using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities.Enums;

namespace Fathom.API.Services;

/// <summary>
/// Generates formatted citations (BibTeX / RIS / APA / MLA) for a Series from its metadata.
/// </summary>
public interface ICitationService
{
    /// <summary>
    /// Generate a formatted citation for a series, or null if the series is not found / not accessible.
    /// </summary>
    Task<string?> GenerateAsync(int userId, int seriesId, CitationFormat format, CancellationToken ct = default);
}
