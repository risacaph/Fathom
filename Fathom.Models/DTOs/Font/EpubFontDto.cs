using Fathom.Models.Entities.Enums.Font;

namespace Fathom.Models.DTOs.Font;

public sealed record EpubFontDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public FontProvider Provider { get; set; }
    public string FileName { get; set; }

}
