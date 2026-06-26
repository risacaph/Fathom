using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.KavitaPlus;

namespace Fathom.API.Services.Plus;

public interface IKavitaPlusProviderHealthService
{
    Task<IList<KavitaPlusProviderHealthSnapshotDto>> GetProviderHealthSnapshot(bool forceCheck = false, CancellationToken ct = default);
}
