using System.Collections.Generic;
using Fathom.Models.DTOs.Collection;

namespace Fathom.Models.DTOs.CollectionTags;

public sealed record UpdateSeriesForTagDto
{
    public AppUserCollectionDto Tag { get; init; } = default!;
    public IEnumerable<int> SeriesIdsToRemove { get; init; } = default!;
}
