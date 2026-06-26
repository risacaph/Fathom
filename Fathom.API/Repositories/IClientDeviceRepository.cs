using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Progress;
using Fathom.Models.Entities.User;

namespace Fathom.API.Repositories;

public interface IClientDeviceRepository
{
    Task<ClientDevice?> GetClientDeviceById(int id, int userId, CancellationToken cancellationToken = default);
    Task<ClientDevice?> GetClientDeviceByClientFingerprint(int userId, string uiFingerprint, CancellationToken cancellationToken);
    Task<IEnumerable<ClientDevice>> GetUserDevicesAsync(int userId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientDeviceDto>> GetUserDeviceDtosAsync(int userId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientDeviceDto>> GetAllUserDeviceDtos(bool includeInactive = false, CancellationToken cancellationToken = default);
}
