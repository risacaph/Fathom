namespace Fathom.Models.DTOs.Search;

/// <summary>
/// A single hit from the full-text content search (FTS5).
/// </summary>
public class FullTextSearchResultDto
{
    public int ChapterId { get; set; }
    public int SeriesId { get; set; }
    public int LibraryId { get; set; }
    /// <summary>
    /// HTML snippet of the matched content with &lt;mark&gt; tags around matched terms.
    /// </summary>
    public string Snippet { get; set; } = string.Empty;
    /// <summary>
    /// Series name, populated for display when available.
    /// </summary>
    public string? SeriesName { get; set; }
}
