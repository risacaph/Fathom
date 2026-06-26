using System.Collections.Generic;

namespace Fathom.Models.DTOs.SideNav;

public sealed record BulkUpdateSideNavStreamVisibilityDto
{
    public required IList<int> Ids { get; set; }
    public required bool Visibility { get; set; }
}
