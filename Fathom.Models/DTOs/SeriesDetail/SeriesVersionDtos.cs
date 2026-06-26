using System;

namespace Fathom.Models.DTOs.SeriesDetail;

/// <summary>
/// Request to set version / supersession info on a Series (document amendment tracking).
/// </summary>
public class UpdateSeriesVersionDto
{
    public int SeriesId { get; set; }
    public string Version { get; set; } = string.Empty;
    /// <summary>Id of the Series that supersedes this one (the newer version), or null to clear.</summary>
    public int? SupersededBySeriesId { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// One node in a document version / amendment chain, ordered oldest → newest.
/// </summary>
public class SeriesVersionNodeDto
{
    public int SeriesId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    /// <summary>True for the most recent (non-superseded) version in the chain.</summary>
    public bool IsCurrent { get; set; }
}
