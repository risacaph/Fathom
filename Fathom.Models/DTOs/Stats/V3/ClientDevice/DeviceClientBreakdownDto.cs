using System.Collections.Generic;
using Fathom.Models.DTOs.Statistics;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Stats.V3.ClientDevice;

public sealed record DeviceClientBreakdownDto
{
    public IList<StatCount<ClientDeviceType>> Records { get; set; }
    public int TotalCount { get; set; }
}
