using Fathom.Models.Entities.Enums;
using Fathom.Models.Entities.Metadata;

namespace Fathom.Models.DTOs;
#nullable enable

public sealed record RatingDto
{

    public int AverageScore { get; set; }
    public int FavoriteCount { get; set; }
    public ScrobbleProvider Provider { get; set; }
    /// <inheritdoc cref="ExternalRating.Authority"/>
    public RatingAuthority Authority { get; set; } = RatingAuthority.User;
    public string? ProviderUrl { get; set; }
}
