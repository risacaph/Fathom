using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities;

namespace Fathom.API.Services;

public interface IFontService
{
    Task<EpubFont> CreateFontFromFileAsync(string path, CancellationToken ct = default);
    Task Delete(int fontId, CancellationToken ct = default);
    Task<EpubFont> CreateFontFromUrl(string url, CancellationToken ct = default);
    Task<bool> IsFontInUse(int fontId, CancellationToken ct = default);
}
