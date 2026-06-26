using System.Collections.Generic;

namespace Fathom.Models.DTOs;

public sealed record CheckForFilesInFolderRootsDto
{
    public ICollection<string> Roots { get; init; }
}
