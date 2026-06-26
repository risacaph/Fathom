using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fathom.API.Repositories;
using Fathom.Common.Helpers;
using Fathom.Models.DTOs.Email;
using Microsoft.EntityFrameworkCore;

namespace Fathom.Database.Repositories;

public class EmailHistoryRepository(DataContext context, IMapper mapper) : IEmailHistoryRepository
{
    public async Task<IList<EmailHistoryDto>> GetEmailDtos(UserParams userParams, CancellationToken ct = default)
    {
        return await context.EmailHistory
            .OrderByDescending(h => h.SendDate)
            .ProjectTo<EmailHistoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
