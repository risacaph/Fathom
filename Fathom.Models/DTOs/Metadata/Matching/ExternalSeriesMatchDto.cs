using Fathom.Models.DTOs.KavitaPlus.Metadata;

namespace Fathom.Models.DTOs.Metadata.Matching;

public sealed record ExternalSeriesMatchDto
{
    public ExternalSeriesDetailDto Series { get; set; }
    public float MatchRating { get; set; }
}
