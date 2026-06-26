using System.Threading;
using System.Threading.Tasks;

namespace Fathom.API.Services;

public interface IActiveUserTrackerService
{
    void RecordActive(int userId);
    Task FlushAsync(CancellationToken ct = default);
}
