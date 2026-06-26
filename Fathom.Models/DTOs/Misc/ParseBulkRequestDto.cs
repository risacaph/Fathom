using System.Collections.Generic;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Misc;

public sealed record ParseBulkRequestDto
{
    public ICollection<string> Names { get; set; }
    public LibraryType LibraryType { get; set; }
}
