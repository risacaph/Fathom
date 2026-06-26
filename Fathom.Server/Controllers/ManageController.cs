using System;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.Common.Helpers;
using Fathom.Models.Constants;
using Fathom.Models.DTOs.KavitaPlus.Manage;
using Fathom.Server.Attributes;
using Fathom.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

/// <summary>
/// All things centered around Managing the Kavita instance, that isn't aligned with an entity
/// </summary>
[Authorize(PolicyGroups.AdminPolicy)]
public class ManageController(IUnitOfWork unitOfWork) : BaseApiController
{
    /// <summary>
    /// Returns a list of all Series that is Kavita+ applicable to metadata match and the status of it
    /// </summary>
    /// <returns></returns>
    [KPlus]
    [Authorize(PolicyGroups.AdminPolicy)]
    [HttpPost("series-metadata")]
    public async Task<ActionResult<PagedList<ManageMatchSeriesDto>>> SeriesMetadata(ManageMatchFilterDto filter, [FromQuery] UserParams? userParams)
    {
        userParams ??= UserParams.Default;

        var res = await unitOfWork.ExternalSeriesMetadataRepository.GetAllSeries(filter, userParams);

        Response.AddPaginationHeader(res);
        return Ok(res);
    }
}
