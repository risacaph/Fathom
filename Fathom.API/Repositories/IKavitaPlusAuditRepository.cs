using System;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Common.Helpers;
using Fathom.Models.DTOs.KavitaPlus;
using Fathom.Models.Entities.History;

namespace Fathom.API.Repositories;

public interface IKavitaPlusAuditRepository
{
    void Add(KavitaPlusAuditLog entry);
    Task DeleteOlderThanAsync(DateTime cutoff, CancellationToken ct = default);

    Task<PagedList<KavitaPlusAuditEntryDto>> GetPagedAsync(
        KavitaPlusAuditFilterDto filter, UserParams userParams, CancellationToken ct = default);

    Task<PagedList<KavitaPlusAuditEntryDto>> GetMyActivityAsync(
        int userId, KavitaPlusAuditFilterDto filter, UserParams userParams, CancellationToken ct = default);

    Task<KavitaPlusAuditStatsDto> GetStatsAsync(CancellationToken ct = default);

    Task<KavitaPlusAuditSeriesInfoDto> GetSeriesInfoAsync(
        int seriesId, int callingUserId, bool isAdmin, CancellationToken ct = default);

    Task MarkAsRetriedAsync(long id, CancellationToken ct = default);
}
