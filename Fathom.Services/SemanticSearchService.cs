using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.DTOs.Search;
using Fathom.Models.Entities;
using Fathom.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// Semantic search / RAG over chapter text (F6). Embeddings are stored in the DocumentChunk table as packed
/// float32 BLOBs; ranking is an in-process cosine scan. This is intentionally storage-engine-agnostic (no
/// native vector extension needed); for very large libraries a dedicated vector index would be the next step.
/// </summary>
public partial class SemanticSearchService(IUnitOfWork unitOfWork, IAiProviderService ai,
    IBookService bookService, ILogger<SemanticSearchService> logger) : ISemanticSearchService
{
    private static readonly LibraryType[] TextLibraryTypes =
        [LibraryType.Book, LibraryType.LightNovel, LibraryType.Regulations, LibraryType.Research];

    private const int TargetChunkChars = 1000;
    private const int ChunkOverlapChars = 150;
    private const int MaxChunksPerChapter = 400;
    private const int EmbedBatchSize = 64;
    private const int SnippetChars = 400;

    public async Task<int> IndexChapterAsync(int chapterId, CancellationToken ct = default)
    {
        var config = await ai.GetConfigAsync(ct);
        if (!config.CanEmbed) throw new AiProviderNotConfiguredException("AI embeddings are not enabled or configured.");

        var info = await unitOfWork.DataContext.Chapter
            .Where(c => c.Id == chapterId)
            .Select(c => new
            {
                c.Id,
                SeriesId = c.Volume.SeriesId,
                LibraryId = c.Volume.Series.LibraryId,
                FilePath = c.Files.Select(f => f.FilePath).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);
        if (info?.FilePath == null) return 0;

        var text = await bookService.ExtractPlainTextAsync(info.FilePath, ct);
        var chunks = ChunkText(text);
        if (chunks.Count > MaxChunksPerChapter) chunks = chunks.Take(MaxChunksPerChapter).ToList();

        await RemoveChapterAsync(chapterId, ct);
        if (chunks.Count == 0) return 0;

        var vectors = new List<float[]>(chunks.Count);
        for (var i = 0; i < chunks.Count; i += EmbedBatchSize)
        {
            ct.ThrowIfCancellationRequested();
            var batch = chunks.GetRange(i, Math.Min(EmbedBatchSize, chunks.Count - i));
            vectors.AddRange(await ai.EmbedAsync(batch, ct));
        }

        for (var i = 0; i < chunks.Count && i < vectors.Count; i++)
        {
            unitOfWork.DataContext.DocumentChunk.Add(new DocumentChunk
            {
                ChapterId = info.Id,
                SeriesId = info.SeriesId,
                LibraryId = info.LibraryId,
                ChunkIndex = i,
                Content = chunks[i],
                Embedding = Pack(vectors[i]),
                EmbeddingModel = config.EmbeddingModel
            });
        }

        await unitOfWork.CommitAsync();
        return chunks.Count;
    }

    public async Task RemoveChapterAsync(int chapterId, CancellationToken ct = default)
    {
        await unitOfWork.DataContext.DocumentChunk
            .Where(c => c.ChapterId == chapterId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> ReindexAllAsync(CancellationToken ct = default)
    {
        if (!await ai.IsEnabledAsync(ct))
        {
            logger.LogInformation("[Semantic] Skipping reindex — AI provider is not enabled");
            return 0;
        }

        var chapterIds = await unitOfWork.DataContext.Chapter
            .Where(c => TextLibraryTypes.Contains(c.Volume.Series.Library.Type))
            .Select(c => c.Id)
            .ToListAsync(ct);

        logger.LogInformation("[Semantic] Reindexing {Count} chapters", chapterIds.Count);
        var indexed = 0;
        foreach (var id in chapterIds)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await IndexChapterAsync(id, ct);
                indexed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Semantic] Failed to index chapter {Id}", id);
            }
        }

        logger.LogInformation("[Semantic] Reindexed {Count} chapters", indexed);
        return indexed;
    }

    public async Task<IList<SemanticSearchResultDto>> SearchAsync(string query, IList<int> libraryIds,
        int limit = 10, CancellationToken ct = default)
    {
        var results = new List<SemanticSearchResultDto>();
        if (string.IsNullOrWhiteSpace(query) || libraryIds.Count == 0) return results;

        var config = await ai.GetConfigAsync(ct);
        if (!config.CanEmbed) throw new AiProviderNotConfiguredException("AI embeddings are not enabled or configured.");

        var queryVec = (await ai.EmbedAsync([query], ct)).FirstOrDefault();
        if (queryVec is not { Length: > 0 }) return results;

        // Phase 1: scan candidate embeddings (no Content) scoped to the accessible libraries.
        var candidates = await unitOfWork.DataContext.DocumentChunk
            .Where(c => libraryIds.Contains(c.LibraryId))
            .Select(c => new { c.Id, c.ChapterId, c.SeriesId, c.LibraryId, c.ChunkIndex, c.Embedding })
            .ToListAsync(ct);

        var ranked = candidates
            .Select(c => new { Row = c, Score = Cosine(queryVec, Unpack(c.Embedding)) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();
        if (ranked.Count == 0) return results;

        // Phase 2: fetch Content only for the winners.
        var ids = ranked.Select(r => r.Row.Id).ToList();
        var contents = await unitOfWork.DataContext.DocumentChunk
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Content })
            .ToDictionaryAsync(c => c.Id, c => c.Content, ct);

        results.AddRange(ranked.Select(r => new SemanticSearchResultDto
        {
            ChapterId = r.Row.ChapterId,
            SeriesId = r.Row.SeriesId,
            LibraryId = r.Row.LibraryId,
            ChunkIndex = r.Row.ChunkIndex,
            Snippet = Truncate(contents.GetValueOrDefault(r.Row.Id, string.Empty), SnippetChars),
            Score = r.Score
        }));

        return results;
    }

    public async Task<RagAnswerDto> AskAsync(string question, IList<int> libraryIds, int topK = 6,
        CancellationToken ct = default)
    {
        var config = await ai.GetConfigAsync(ct);
        if (!config.IsConfigured) throw new AiProviderNotConfiguredException("AI provider is not enabled or configured.");

        var hits = await SearchAsync(question, libraryIds, topK, ct);
        if (hits.Count == 0)
            return new RagAnswerDto { Answer = string.Empty, HasContext = false, Model = config.ChatModel };

        var context = new StringBuilder();
        var used = new List<SemanticSearchResultDto>();
        foreach (var h in hits)
        {
            var block = $"[S{h.SeriesId}/C{h.ChapterId}] {h.Snippet}\n\n";
            if (context.Length + block.Length > config.MaxContextChars) break;
            context.Append(block);
            used.Add(h);
        }

        const string system =
            "You answer questions strictly from the supplied sources, which come from the user's own library. " +
            "Cite the sources you use inline using their bracket tags, e.g. [S12/C34]. " +
            "If the sources do not contain the answer, say you couldn't find it in the library — do not invent facts.";
        var user = $"Sources:\n{context}\nQuestion: {question}";

        var answer = await ai.ChatAsync(system, user, ct);
        return new RagAnswerDto { Answer = answer, Sources = used, Model = config.ChatModel, HasContext = true };
    }

    /// <summary>Normalize whitespace and split into overlapping, word-boundary-aligned chunks.</summary>
    private static List<string> ChunkText(string text)
    {
        var clean = WhitespaceRegex().Replace(text ?? string.Empty, " ").Trim();
        var chunks = new List<string>();
        if (clean.Length == 0) return chunks;

        var pos = 0;
        while (pos < clean.Length)
        {
            var len = Math.Min(TargetChunkChars, clean.Length - pos);
            // Extend to the next space so we don't cut a word in half (unless we're at the end).
            if (pos + len < clean.Length)
            {
                var nextSpace = clean.IndexOf(' ', pos + len);
                len = nextSpace > 0 ? nextSpace - pos : clean.Length - pos;
            }

            var chunk = clean.Substring(pos, len).Trim();
            if (chunk.Length > 0) chunks.Add(chunk);

            if (pos + len >= clean.Length) break;
            pos += Math.Max(1, len - ChunkOverlapChars);
        }

        return chunks;
    }

    private static byte[] Pack(float[] vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static float[] Unpack(byte[] bytes)
    {
        var vector = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, vector, 0, vector.Length * sizeof(float));
        return vector;
    }

    private static double Cosine(float[] a, float[] b)
    {
        if (a.Length == 0 || a.Length != b.Length) return 0;
        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value ?? string.Empty : value[..max];

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
