using System.Collections.Generic;

namespace Fathom.Models.DTOs.Device.EmailDevice;

public sealed record SendToEmailDeviceDto
{
    public int DeviceId { get; set; }
    public IReadOnlyList<int> ChapterIds { get; set; } = default!;
}
