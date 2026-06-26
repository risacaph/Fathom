using System.ComponentModel.DataAnnotations;

namespace Fathom.Models.DTOs.ReadingLists.Request;

/// <summary>
/// DTO for moving a reading list item to another position within the same list
/// </summary>
public sealed record UpdateReadingListPosition
{
    [Required] public int ReadingListId { get; set; }
    [Required] public int ReadingListItemId { get; set; }
    public int FromPosition { get; set; }
    [Required] public int ToPosition { get; set; }
}
