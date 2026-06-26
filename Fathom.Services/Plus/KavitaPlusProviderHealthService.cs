using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using Flurl.Http;
using Fathom.API.Services.Plus;
using Fathom.Models.Constants;
using Fathom.Models.DTOs.KavitaPlus;
using Microsoft.Extensions.Logging;

namespace Fathom.Services.Plus;

public class KavitaPlusProviderHealthService(
    IEasyCachingProviderFactory cachingProviderFactory,
    IKavitaPlusApiService kavitaPlusApiService,
    ILogger<KavitaPlusProviderHealthService> logger)
    : IKavitaPlusProviderHealthService
{
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(45);
    private const string CacheKey = "provider-health";

    public async Task<IList<KavitaPlusProviderHealthSnapshotDto>> GetProviderHealthSnapshot(bool forceCheck = false, CancellationToken ct = default)
    {
        var provider = cachingProviderFactory.GetCachingProvider(EasyCacheProfiles.ProviderHealth);

        if (!forceCheck)
        {
            var cached = await provider.GetAsync<IList<KavitaPlusProviderHealthSnapshotDto>>(CacheKey, ct);
            if (cached.HasValue) return cached.Value;
        }

        try
        {
            var response = await kavitaPlusApiService.GetProviderHealthSnapshot(ct);
            await provider.FlushAsync(ct);
            await provider.SetAsync(CacheKey, response, _cacheTimeout, ct);
            return response;
        }
        catch (FlurlHttpException e)
        {
            logger.LogError(e, "An error happened during the request to Kavita+ API");
        }

        return [];
    }
}
