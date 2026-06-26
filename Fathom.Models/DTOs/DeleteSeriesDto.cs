using System.Collections.Generic;

namespace Fathom.Models.DTOs;

public sealed record DeleteSeriesDto
{
    public IList<int> SeriesIds { get; set; } = default!;
}
