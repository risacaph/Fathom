using System.Collections.Generic;

namespace Fathom.Models.DTOs.Statistics;

public sealed record SpreadStatsDto
{
    public List<StatBucketDto> Buckets { get; set; }
    public int TotalCount { get; set; }
}
