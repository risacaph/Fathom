using System.Collections.Generic;
using System.Threading.Tasks;
using Fathom.Models.DTOs.KavitaPlus.Metadata;
using Fathom.Models.Entities;
using Fathom.Models.Parser;

namespace Fathom.API.Services.Scanner;

public sealed record ProcessSeriesArgs
{
    public required Library Library { get; init; }
    public required int TotalToProcess { get; init; }
    public required int LeftToProcess { get; init; }
    public bool ForceUpdate { get; init; } = false;
}

public interface IProcessSeries
{
    Task<int?> ProcessSeriesAsync(MetadataSettingsDto settings, IList<ParserInfo> parsedInfos, ProcessSeriesArgs args);
}
