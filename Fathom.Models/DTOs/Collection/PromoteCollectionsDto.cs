using System.Collections.Generic;

namespace Fathom.Models.DTOs.Collection;

public class PromoteCollectionsDto
{
    public IList<int> CollectionIds { get; init; }
    public bool Promoted { get; init; }
}
