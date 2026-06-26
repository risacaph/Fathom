using System.Collections.Generic;

namespace Fathom.Models.DTOs.ReadingLists.CBL.Import;

public class CblRepoImportRequestDto
{
    public IList<CblRepoItemDto> Items { get; set; } = [];
}
