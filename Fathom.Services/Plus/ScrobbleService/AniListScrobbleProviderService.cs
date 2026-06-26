using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services.Plus;
using Fathom.Common;
using Fathom.Common.Helpers;
using Fathom.Models.DTOs.KavitaPlus.Scrobble;
using Fathom.Models.DTOs.Scrobbling;
using Fathom.Models.Entities;
using Fathom.Models.Entities.Enums;
using Fathom.Models.Entities.Enums.KavitaPlus;
using Fathom.Models.Entities.Scrobble;
using Fathom.Models.Entities.User;
using Microsoft.Extensions.Logging;

namespace Fathom.Services.Plus.ScrobbleService;

public class AniListScrobbleProviderService(ILogger<AniListScrobbleProviderService> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService)
    : SeriesScrobbleService<AniListScrobbleProviderService>(logger, unitOfWork, auditService)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.AniList;
    protected override IReadOnlyList<ScrobbleEventType> SupportedEvents =>
    [
        ScrobbleEventType.ChapterRead, ScrobbleEventType.AddWantToRead, ScrobbleEventType.RemoveWantToRead,
        ScrobbleEventType.ScoreUpdated, ScrobbleEventType.Review, ScrobbleEventType.ReadStatusUpdate
    ];
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series)
    {
        evt.AniListId = series.AniListId;
    }

    // AniList's rate limit is enforced server-wide (~30 requests/min), shared across all users.
    // 30/min == one request every 2s
    public override RateProfile RateProfile => new(
        BaseInterval: TimeSpan.FromSeconds(2),
        Buffer: TimeSpan.FromMilliseconds(300),
        LowRateThreshold: 10,
        RebuildWait: TimeSpan.FromSeconds(60),
        Scope: RateScope.Server);

    public override bool IsTokenValid(string token)
    {
        return JwtHelper.IsTokenValid(token);
    }
}
