namespace Fathom.Models.DTOs.Reader;

public sealed record RemoveBookmarkForSeriesDto
{
    public int SeriesId { get; init; }
}
