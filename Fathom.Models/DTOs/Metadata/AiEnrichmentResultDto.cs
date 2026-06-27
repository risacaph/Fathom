using System;
using System.Collections.Generic;

namespace Fathom.Models.DTOs.Metadata;

/// <summary>
/// AI-generated metadata suggestions for a series (F8). These are <em>suggestions</em> for an admin to review
/// and apply — nothing is written to the series automatically.
/// </summary>
public class AiEnrichmentResultDto
{
    public int SeriesId { get; set; }
    public string SeriesName { get; set; } = string.Empty;

    /// <summary>A short, neutral marketing-style summary (2–4 sentences).</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>A single-sentence tagline / hook.</summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>Suggested genre names.</summary>
    public List<string> Genres { get; set; } = [];

    /// <summary>Suggested free-form tags / keywords.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Model id that produced the suggestion (for provenance).</summary>
    public string Model { get; set; } = string.Empty;

    public DateTime GeneratedUtc { get; set; }
}
