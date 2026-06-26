using System.Collections.Generic;

namespace Fathom.Models.DTOs.Metadata;

/// <summary>
/// Academic metadata resolved from an external provider (CrossRef) for a DOI. Used to preview/apply
/// metadata onto a Series in Research / Regulations libraries.
/// </summary>
public class AcademicMetadataDto
{
    public string Doi { get; set; } = string.Empty;
    public string? Title { get; set; }
    public IList<string> Authors { get; set; } = [];
    public int? Year { get; set; }
    public string? Abstract { get; set; }
    /// <summary>Journal or conference name (CrossRef container-title).</summary>
    public string? Venue { get; set; }
    public string? Publisher { get; set; }
    /// <summary>e.g. journal-article, proceedings-article, book-chapter.</summary>
    public string? Type { get; set; }
    public string? Url { get; set; }
}
