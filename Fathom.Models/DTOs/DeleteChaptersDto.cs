using System.Collections.Generic;

namespace Fathom.Models.DTOs;

public sealed record DeleteChaptersDto
{
    public IList<int> ChapterIds { get; set; } = default!;
}
