using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.KavitaPlus.OAuth;
using Fathom.Models.Entities.User;

namespace Fathom.API.Services.Plus;

public interface IOAuthService
{
    Task HandleCallback(AppUser user, OAuthUpstream upstream, string token, string? refreshToken = null);

    Task RefreshTokens(CancellationToken ct = default);
}
