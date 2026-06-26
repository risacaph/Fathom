using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities.Progress;

namespace Fathom.API.Services;

public interface IDeviceTrackingService
{
    Task<int> TrackDeviceAsync(int userId, ClientInfoData clientInfo, string? uiFingerprint, CancellationToken ct);
    Task ClearDeviceCacheAsync(int deviceId);
    Task ClearUserDeviceCachesAsync(int userId);
}
