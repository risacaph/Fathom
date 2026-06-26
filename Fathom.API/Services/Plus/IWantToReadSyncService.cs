using System.Threading;
using System.Threading.Tasks;

namespace Fathom.API.Services.Plus;

public interface IWantToReadSyncService
{
    Task Sync(CancellationToken ct = default);
}
