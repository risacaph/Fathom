using System;
using Fathom.Models.DTOs.KavitaPlus.Audit;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.KavitaPlus;

public sealed record KavitaPlusAuditSystemDetailsDto
{

    public ScrobbleProvider Provider { get; init; }
    public DateTime? ValidUntilUtc { get; init; }
    public KavitaPlusUserInfo? UserInfo { get; init; }

    public static KavitaPlusAuditSystemDetailsDto From(AuditLogSystemTokenRefreshParamsDto dto)
    {
        return new KavitaPlusAuditSystemDetailsDto
        {
            Provider = dto.Provider,
            ValidUntilUtc = dto.ValidUntilUtc,
        };
    }

    public static KavitaPlusAuditSystemDetailsDto From(AuditLogSystemProviderInfoSyncParamsDto dto)
    {
        return new KavitaPlusAuditSystemDetailsDto
        {
            Provider = dto.Provider,
            UserInfo = dto.UserInfo,
        };
    }

}
