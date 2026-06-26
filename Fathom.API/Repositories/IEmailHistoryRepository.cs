using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Common.Helpers;
using Fathom.Models.DTOs.Email;

namespace Fathom.API.Repositories;

public interface IEmailHistoryRepository
{
    Task<IList<EmailHistoryDto>> GetEmailDtos(UserParams userParams, CancellationToken ct = default);
}
