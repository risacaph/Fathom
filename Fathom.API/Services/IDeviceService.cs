using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Device.EmailDevice;
using Fathom.Models.Entities;
using Fathom.Models.Entities.User;

namespace Fathom.API.Services;

public interface IDeviceService
{
    Task<Device?> Create(CreateEmailDeviceDto dto, AppUser userWithDevices, CancellationToken ct = default);
    Task<Device?> Update(UpdateEmailDeviceDto dto, AppUser userWithDevices, CancellationToken ct = default);
    Task<bool> Delete(AppUser userWithDevices, int deviceId, CancellationToken ct = default);
    Task<bool> SendTo(IReadOnlyList<int> chapterIds, int deviceId, CancellationToken ct = default);
}
