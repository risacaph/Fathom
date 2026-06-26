using Fathom.Models.Entities.Enums;

namespace Fathom.Models.Entities;

public class LibraryFileTypeGroup
{
    public int Id { get; set; }
    public FileTypeGroup FileTypeGroup { get; set; }

    public int LibraryId { get; set; }
    public Library Library { get; set; } = null!;
}
