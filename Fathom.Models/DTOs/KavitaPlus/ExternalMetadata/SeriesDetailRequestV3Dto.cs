using System.Collections.Generic;
using Fathom.Models.DTOs.Scrobbling;
using Fathom.Models.Entities.Enums;
using Fathom.Models.Entities.Enums.KavitaPlus;

namespace Fathom.Models.DTOs.KavitaPlus.ExternalMetadata;

public sealed record SeriesDetailRequestV3Dto: MetadataRequest
{
    public required MetadataProvider Provider { get; set; }
    public required string SeriesName { get; set; }
    public List<string> AlternativeNames { get; set; } = [];
    public PlusMediaFormat Format { get; set; }
    public int? ChapterCount { get; set; }
    public int? VolumeCount { get; set; }
    public int? Year { get; set; }
}
