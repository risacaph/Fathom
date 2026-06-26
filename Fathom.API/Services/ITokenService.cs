using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Account;
using Fathom.Models.Entities.User;

namespace Fathom.API.Services;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user, CancellationToken ct = default);
    Task<TokenRequestDto?> ValidateRefreshToken(TokenRequestDto request, CancellationToken ct = default);
    Task<string> CreateRefreshToken(AppUser user, CancellationToken ct = default);
    Task<string?> GetJwtFromUser(AppUser user, CancellationToken ct = default);
}
