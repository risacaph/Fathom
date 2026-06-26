using System.Collections.Generic;
using Fathom.Models.DTOs.KavitaPlus.Metadata;
using Fathom.Models.DTOs.Scrobbling;
using Fathom.Models.DTOs.SeriesDetail;

namespace Fathom.Models.DTOs.KavitaPlus.ExternalMetadata;
#nullable enable

public sealed record SeriesDetailPlusApiDto
{
    public IEnumerable<MediaRecommendationDto> Recommendations { get; set; }
    public IEnumerable<UserReviewDto> Reviews { get; set; }
    public IEnumerable<RatingDto> Ratings { get; set; }
    public ExternalSeriesDetailDto? Series { get; set; }
    public int? AniListId { get; set; }
    public long? MalId { get; set; }
    public int? MangabakaId { get; set; }
    public int? HardCoverId { get; set; }
    public int? CbrId { get; set; }
}
