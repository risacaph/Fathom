using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Koreader;

namespace Fathom.API.Services;

public interface IKoreaderService
{
    Task SaveProgress(KoreaderBookDto koreaderBookDto, int userId, CancellationToken ct = default);
    Task<KoreaderBookDto> GetProgress(string bookHash, int userId, CancellationToken ct = default);
}
