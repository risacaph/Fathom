using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Search;

namespace Fathom.API.Services;

/// <summary>
/// Semantic search and retrieval-augmented generation over book/PDF text (F6). Chapter text is chunked and
/// embedded with the configured AI provider; queries are embedded and matched by cosine similarity. Requires
/// an AI provider with an embedding model configured.
/// </summary>
public interface ISemanticSearchService
{
    /// <summary>Chunk, embed and (re)index a single chapter. Returns the number of chunks stored.</summary>
    Task<int> IndexChapterAsync(int chapterId, CancellationToken ct = default);

    /// <summary>Drop all stored chunks/embeddings for a chapter (e.g. on delete or re-scan).</summary>
    Task RemoveChapterAsync(int chapterId, CancellationToken ct = default);

    /// <summary>(Re)index every chapter in text-based libraries. Returns the number of chapters indexed.</summary>
    Task<int> ReindexAllAsync(CancellationToken ct = default);

    /// <summary>Vector-similarity search scoped to the given libraries, ranked best-first.</summary>
    Task<IList<SemanticSearchResultDto>> SearchAsync(string query, IList<int> libraryIds, int limit = 10,
        CancellationToken ct = default);

    /// <summary>Answer a question grounded in the user's library (RAG): retrieve top chunks, then synthesize.</summary>
    Task<RagAnswerDto> AskAsync(string question, IList<int> libraryIds, int topK = 6, CancellationToken ct = default);
}
