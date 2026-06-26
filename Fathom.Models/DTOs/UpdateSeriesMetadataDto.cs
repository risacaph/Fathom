using Fathom.Models.DTOs.Common;

namespace Fathom.Models.DTOs;

public sealed record UpdateSeriesMetadataDto
{
    public SeriesMetadataDto SeriesMetadata { get; set; } = null!;
}
