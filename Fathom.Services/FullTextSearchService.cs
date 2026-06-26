using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.DTOs.Search;
using Fathom.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// Full-text content search backed by a SQLite FTS5 virtual table (ChapterFts). The table is created by an
/// EF migration via raw SQL and is not modelled as an EF entity, so reads/writes here use raw SQL.
/// </summary>
public class FullTextSearchService(IUnitOfWork unitOfWork, IBookService bookService,
    ILogger<FullTextSearchService> logger) : IFullTextSearchService
{
    private static readonly LibraryType[] TextLibraryTypes =
        [LibraryType.Book, LibraryType.LightNovel, LibraryType.Regulations, LibraryType.Research];

    public async Task IndexChapterAsync(int chapterId, CancellationToken ct = default)
    {
        var info = await unitOfWork.DataContext.Chapter
            .Where(c => c.Id == chapterId)
            .Select(c => new
            {
                c.Id,
                c.TitleName,
                SeriesId = c.Volume.SeriesId,
                LibraryId = c.Volume.Series.LibraryId,
                FilePath = c.Files.Select(f => f.FilePath).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (info?.FilePath == null) return;

        var content = await bookService.ExtractPlainTextAsync(info.FilePath, ct);

        await RemoveChapterAsync(chapterId, ct);
        await unitOfWork.DataContext.Database.ExecuteSqlRawAsync(
            "INSERT INTO ChapterFts (Title, Content, ChapterId, SeriesId, LibraryId) VALUES ({0}, {1}, {2}, {3}, {4})",
            [info.TitleName ?? string.Empty, content ?? string.Empty, info.Id, info.SeriesId, info.LibraryId], ct);
    }

    public async Task RemoveChapterAsync(int chapterId, CancellationToken ct = default)
    {
        await unitOfWork.DataContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM ChapterFts WHERE ChapterId = {0}", [chapterId], ct);
    }

    public async Task<int> ReindexAllAsync(CancellationToken ct = default)
    {
        var chapterIds = await unitOfWork.DataContext.Chapter
            .Where(c => TextLibraryTypes.Contains(c.Volume.Series.Library.Type))
            .Select(c => c.Id)
            .ToListAsync(ct);

        logger.LogInformation("[FullText] Reindexing {Count} chapters", chapterIds.Count);
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
                logger.LogError(ex, "[FullText] Failed to index chapter {Id}", id);
            }
        }

        logger.LogInformation("[FullText] Reindexed {Count} chapters", indexed);
        return indexed;
    }

    public async Task<IList<FullTextSearchResultDto>> SearchAsync(int userId, string query,
        IList<int> libraryIds, int limit = 50, CancellationToken ct = default)
    {
        var results = new List<FullTextSearchResultDto>();
        var ftsQuery = SanitizeQuery(query);
        if (string.IsNullOrWhiteSpace(ftsQuery) || libraryIds.Count == 0) return results;

        var conn = unitOfWork.DataContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        var libParams = libraryIds.Select((_, i) => "@lib" + i).ToList();
        cmd.CommandText =
            "SELECT ChapterId, SeriesId, LibraryId, snippet(ChapterFts, 1, '<mark>', '</mark>', '…', 12) AS Snippet " +
            $"FROM ChapterFts WHERE ChapterFts MATCH @q AND LibraryId IN ({string.Join(",", libParams)}) " +
            "ORDER BY rank LIMIT @limit";
        AddParam(cmd, "@q", ftsQuery);
        AddParam(cmd, "@limit", limit);
        for (var i = 0; i < libraryIds.Count; i++) AddParam(cmd, "@lib" + i, libraryIds[i]);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new FullTextSearchResultDto
            {
                ChapterId = reader.GetInt32(0),
                SeriesId = reader.GetInt32(1),
                LibraryId = reader.GetInt32(2),
                Snippet = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
            });
        }

        return results;
    }

    private static void AddParam(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    /// <summary>
    /// Turns a free-text query into a safe FTS5 MATCH expression: each token is quoted (so FTS5 operators in
    /// user input can't break the query), AND-ed together, with a prefix match on the final token for type-ahead.
    /// </summary>
    private static string SanitizeQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return string.Empty;

        var tokens = query
            .Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Replace("\"", " ").Trim())
            .Where(t => t.Length > 0)
            .ToList();
        if (tokens.Count == 0) return string.Empty;

        var quoted = tokens.Select((t, i) => i == tokens.Count - 1 ? $"\"{t}\"*" : $"\"{t}\"");
        return string.Join(" AND ", quoted);
    }
}
