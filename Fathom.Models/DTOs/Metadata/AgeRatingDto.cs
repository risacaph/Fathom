using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Metadata;

public sealed record AgeRatingDto
{
    public AgeRating Value { get; set; }
    public required string Title { get; set; }
}
