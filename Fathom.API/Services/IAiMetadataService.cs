using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Metadata;

namespace Fathom.API.Services;

/// <summary>
/// AI metadata enrichment (F8): uses the configured AI provider to propose a summary, tagline, genres and tags
/// for a series based on its title and a sample of its text. Returns suggestions for admin review; it does not
/// mutate the series.
/// </summary>
public interface IAiMetadataService
{
    /// <summary>
    /// Generate metadata suggestions for the given series. Throws
    /// <see cref="AiProviderNotConfiguredException"/> when AI is disabled.
    /// </summary>
    Task<AiEnrichmentResultDto> EnrichSeriesAsync(int seriesId, CancellationToken ct = default);
}
