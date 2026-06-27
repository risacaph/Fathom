using System.Collections.Generic;

namespace Fathom.Models.DTOs.Search;

/// <summary>
/// A retrieval-augmented answer (F6): a natural-language answer grounded in the user's own library, plus the
/// source chunks it was drawn from so the answer is traceable.
/// </summary>
public class RagAnswerDto
{
    /// <summary>The model's answer, constrained to the retrieved context.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>The chunks retrieved and supplied to the model as context.</summary>
    public List<SemanticSearchResultDto> Sources { get; set; } = [];

    /// <summary>Chat model id that produced the answer.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>False when retrieval found nothing relevant (the model was not called).</summary>
    public bool HasContext { get; set; }
}
