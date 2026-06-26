using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Device.EmailDevice;
using Fathom.Models.Entities;

namespace Fathom.API.Repositories;

public interface IDeviceRepository
{
    void Update(Device device);
    Task<IList<EmailDeviceDto>> GetDevicesForUserAsync(int userId, CancellationToken ct = default);
    Task<Device?> GetDeviceById(int deviceId, CancellationToken ct = default);
}
