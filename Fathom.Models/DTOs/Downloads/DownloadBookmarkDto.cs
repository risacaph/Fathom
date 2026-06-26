using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fathom.Models.DTOs.Reader;

namespace Fathom.Models.DTOs.Downloads;

public sealed record DownloadBookmarkDto
{
    [Required]
    public IEnumerable<BookmarkDto> Bookmarks { get; set; } = default!;
}
