using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Search;

namespace Fathom.API.Services;

/// <summary>
/// Full-text content search over book/PDF text using a SQLite FTS5 virtual table.
/// </summary>
public interface IFullTextSearchService
{
    /// <summary>Extract and (re)index the text content of a single chapter.</summary>
    Task IndexChapterAsync(int chapterId, CancellationToken ct = default);

    /// <summary>Remove a chapter from the full-text index (e.g. on delete).</summary>
    Task RemoveChapterAsync(int chapterId, CancellationToken ct = default);

    /// <summary>Rebuild the index for all chapters in text-based libraries. Returns the number indexed.</summary>
    Task<int> ReindexAllAsync(CancellationToken ct = default);

    /// <summary>Search indexed content, scoped to the libraries the user can access.</summary>
    Task<IList<FullTextSearchResultDto>> SearchAsync(int userId, string query, IList<int> libraryIds,
        int limit = 50, CancellationToken ct = default);
}
