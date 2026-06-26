using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.DTOs.SeriesDetail;
using Microsoft.EntityFrameworkCore;

namespace Fathom.Services;

/// <summary>
/// Tracks document versions / amendments via Series.SupersededBySeriesId (a plain self-reference id).
/// </summary>
public class DocumentVersionService(IUnitOfWork unitOfWork) : IDocumentVersionService
{
    public async Task<bool> SetVersionAsync(UpdateSeriesVersionDto dto, CancellationToken ct = default)
    {
        var series = await unitOfWork.SeriesRepository.GetSeriesByIdAsync(dto.SeriesId);
        if (series == null) return false;

        series.Version = dto.Version ?? string.Empty;
        series.SupersededBySeriesId = dto.SupersededBySeriesId;
        series.EffectiveDate = dto.EffectiveDate;

        unitOfWork.SeriesRepository.Update(series);
        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task<IList<SeriesVersionNodeDto>> GetChainAsync(int seriesId, CancellationToken ct = default)
    {
        var all = await unitOfWork.DataContext.Series
            .Select(s => new { s.Id, s.Name, s.Version, s.EffectiveDate, s.SupersededBySeriesId })
            .ToListAsync(ct);

        var byId = all.ToDictionary(s => s.Id);
        if (!byId.ContainsKey(seriesId)) return [];

        // predecessor map: newerSeriesId -> olderSeriesId (the one that points to it)
        var predecessor = all
            .Where(s => s.SupersededBySeriesId != null && byId.ContainsKey(s.SupersededBySeriesId.Value))
            .GroupBy(s => s.SupersededBySeriesId!.Value)
            .ToDictionary(g => g.Key, g => g.First().Id);

        // walk back to the oldest version in the chain
        var cur = seriesId;
        var guard = 0;
        while (predecessor.TryGetValue(cur, out var older) && guard++ < 256) cur = older;

        // walk forward from oldest, collecting each version
        var nodes = new List<SeriesVersionNodeDto>();
        var node = cur;
        guard = 0;
        while (guard++ < 256)
        {
            var s = byId[node];
            nodes.Add(new SeriesVersionNodeDto
            {
                SeriesId = s.Id,
                Name = s.Name,
                Version = s.Version,
                EffectiveDate = s.EffectiveDate,
                IsCurrent = s.SupersededBySeriesId == null
            });
            if (s.SupersededBySeriesId == null || !byId.ContainsKey(s.SupersededBySeriesId.Value)) break;
            node = s.SupersededBySeriesId.Value;
        }

        return nodes;
    }
}
