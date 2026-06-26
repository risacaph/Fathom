#nullable enable
using System;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.KavitaPlus.Audit;

public sealed record AuditLogSystemTokenRefreshParamsDto
{
    public ScrobbleProvider Provider { get; init; }
    public DateTime? ValidUntilUtc { get; init; }
}

public sealed record AuditLogSystemProviderInfoSyncParamsDto
{
    public ScrobbleProvider Provider { get; init; }
    public KavitaPlusUserInfo? UserInfo { get; init; }
}

