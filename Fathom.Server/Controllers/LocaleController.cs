using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Fathom.API.Services;
using Fathom.Common.EnvironmentInfo;
using Fathom.Models.Constants;
using Fathom.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

public class LocaleController(
    ILocalizationService localizationService,
    IEasyCachingProviderFactory cachingProviderFactory)
    : BaseApiController
{
    private readonly IEasyCachingProvider _localeCacheProvider = cachingProviderFactory.GetCachingProvider(EasyCacheProfiles.LocaleOptions);

    private static readonly string CacheKey = "locales_" + BuildInfo.Version;

    /// <summary>
    /// Returns all applicable locales on the server
    /// </summary>
    /// <remarks>This can be cached as it will not change per version.</remarks>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FathomLocale>>> GetAllLocales()
    {
        var result = await _localeCacheProvider.GetAsync<IEnumerable<FathomLocale>>(CacheKey);
        if (result.HasValue)
        {
            return Ok(result.Value);
        }

        var ret = localizationService.GetLocales().Where(l => l.TranslationCompletion > 0f);
        await _localeCacheProvider.SetAsync(CacheKey, ret, TimeSpan.FromDays(1));

        return Ok(ret);
    }
}
