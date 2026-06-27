namespace Fathom.Models.DTOs.Search;

/// <summary>
/// A single semantic (vector-similarity) search hit — a chunk of a chapter ranked by cosine similarity (F6).
/// </summary>
public class SemanticSearchResultDto
{
    public int ChapterId { get; set; }
    public int SeriesId { get; set; }
    public int LibraryId { get; set; }

    /// <summary>Order of the matching chunk within its chapter.</summary>
    public int ChunkIndex { get; set; }

    /// <summary>The chunk text (already truncated for display).</summary>
    public string Snippet { get; set; } = string.Empty;

    /// <summary>Cosine similarity in [-1, 1]; higher is more relevant.</summary>
    public double Score { get; set; }
}
