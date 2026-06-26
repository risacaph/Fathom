using System.Threading;
using System.Threading.Tasks;

namespace Fathom.API.Services.Reading;

public interface IReadingHistoryService
{
    Task AggregateYesterdaysActivity(CancellationToken ct = default);
}
