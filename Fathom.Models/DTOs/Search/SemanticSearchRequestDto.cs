namespace Fathom.Models.DTOs.Search;

/// <summary>Request body for a semantic (vector-similarity) search (F6).</summary>
public class SemanticSearchRequestDto
{
    public string Query { get; set; } = string.Empty;

    /// <summary>Maximum number of chunk hits to return.</summary>
    public int Limit { get; set; } = 10;
}

/// <summary>Request body for a retrieval-augmented question over the user's library (F6 RAG).</summary>
public class RagRequestDto
{
    public string Question { get; set; } = string.Empty;

    /// <summary>How many chunks to retrieve as grounding context.</summary>
    public int TopK { get; set; } = 6;
}
