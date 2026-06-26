using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Progress;

namespace Fathom.API.Repositories;

public interface IReadingSessionRepository
{
    Task<IList<ReadingSessionDto>> GetAllReadingSessionAsync(bool isActiveOnly = true, CancellationToken ct = default);
}
