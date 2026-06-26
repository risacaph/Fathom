using System.Collections.Generic;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.Common.Helpers;
using Fathom.Models.Constants;
using Fathom.Models.DTOs.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

[Authorize(Policy = PolicyGroups.AdminPolicy)]
public class EmailController(IUnitOfWork unitOfWork) : BaseApiController
{
    [HttpGet("all")]
    public async Task<ActionResult<IList<EmailHistoryDto>>> GetEmails()
    {
        return Ok(await unitOfWork.EmailHistoryRepository.GetEmailDtos(UserParams.Default));
    }
}
