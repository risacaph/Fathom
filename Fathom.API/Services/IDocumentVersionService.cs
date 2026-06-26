using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.SeriesDetail;

namespace Fathom.API.Services;

/// <summary>
/// Document version / amendment tracking for Series (e.g. superseded maritime regulations).
/// </summary>
public interface IDocumentVersionService
{
    /// <summary>Set version label / supersession / effective date on a series. Returns false if not found.</summary>
    Task<bool> SetVersionAsync(UpdateSeriesVersionDto dto, CancellationToken ct = default);

    /// <summary>Return the full version chain containing the series, ordered oldest → newest.</summary>
    Task<IList<SeriesVersionNodeDto>> GetChainAsync(int seriesId, CancellationToken ct = default);
}
