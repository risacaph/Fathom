using System;
using System.Collections.Generic;
using Fathom.API.Database;
using Fathom.API.Services.Plus;
using Fathom.Common.Helpers;
using Fathom.Models.Entities;
using Fathom.Models.Entities.Enums;
using Fathom.Models.Entities.Enums.KavitaPlus;
using Fathom.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Fathom.Services.Plus.ScrobbleService;

public class MyAnimeListScrobbleProviderService(ILogger<MyAnimeListScrobbleProviderService> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService)
    : SeriesScrobbleService<MyAnimeListScrobbleProviderService>(logger, unitOfWork, auditService)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.Mal;
    protected override IReadOnlyList<ScrobbleEventType> SupportedEvents =>
    [
        ScrobbleEventType.AddWantToRead, ScrobbleEventType.ChapterRead, ScrobbleEventType.ReadStatusUpdate,
        ScrobbleEventType.RemoveWantToRead, ScrobbleEventType.ScoreUpdated
    ];

    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series)
    {
        evt.MalId = series.MalId;
    }

    public override RateProfile RateProfile => new(
        BaseInterval: TimeSpan.FromSeconds(1),
        Buffer: TimeSpan.FromMilliseconds(500),
        LowRateThreshold: 5,
        RebuildWait: TimeSpan.FromSeconds(60),
        Scope: RateScope.Server);

    public override bool IsTokenValid(string token)
    {
        return JwtHelper.IsTokenValid(token);
    }
}
