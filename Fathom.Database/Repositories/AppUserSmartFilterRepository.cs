using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fathom.API.Repositories;
using Fathom.Common.Helpers;
using Fathom.Database.Extensions;
using Fathom.Models.DTOs.Dashboard;
using Fathom.Models.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Fathom.Database.Repositories;

public class AppUserSmartFilterRepository(DataContext context, IMapper mapper) : IAppUserSmartFilterRepository
{
    public void Update(AppUserSmartFilter filter)
    {
        context.Entry(filter).State = EntityState.Modified;
    }

    public void Attach(AppUserSmartFilter filter)
    {
        context.AppUserSmartFilter.Attach(filter);
    }

    public void Delete(AppUserSmartFilter filter)
    {
        context.AppUserSmartFilter.Remove(filter);
    }

    public async Task<IList<SmartFilterDto>> GetAllDtosByUserId(int userId, CancellationToken ct = default)
    {
        return await context.AppUserSmartFilter
            .Where(f => f.AppUserId == userId)
            .ProjectTo<SmartFilterDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public Task<PagedList<SmartFilterDto>> GetPagedDtosByUserIdAsync(int userId, UserParams userParams,
        CancellationToken ct = default)
    {
        var filters = context.AppUserSmartFilter
            .Where(f => f.AppUserId == userId)
            .ProjectTo<SmartFilterDto>(mapper.ConfigurationProvider);

        return PagedList<SmartFilterDto>.CreateAsync(filters, userParams, ct);
    }

    public async Task<AppUserSmartFilter?> GetById(int smartFilterId, CancellationToken ct = default)
    {
        return await context.AppUserSmartFilter
            .FirstOrDefaultAsync(d => d.Id == smartFilterId, ct);
    }
}
