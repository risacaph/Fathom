using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Reader;

namespace Fathom.API.Services;

/// <summary>
/// Backing service for the protected (DRM-lite) document viewer. Renders individual pages as per-user
/// watermarked images so the source file never leaves the server. Pairs with the desktop viewer (MSI),
/// which additionally blocks screen capture and disables copy/print/save on the client.
/// </summary>
public interface IProtectedReaderService
{
    /// <summary>Page count + title for a chapter the caller is allowed to read.</summary>
    Task<ProtectedDocumentInfoDto> GetInfoAsync(int chapterId, CancellationToken ct = default);

    /// <summary>
    /// Render a single page as a JPEG with <paramref name="watermark"/> tiled across it (identifying the
    /// viewer, for leak traceability). Returns the rendered bytes only — never a path to the source file.
    /// </summary>
    Task<byte[]> RenderWatermarkedPageAsync(int chapterId, int page, string watermark, CancellationToken ct = default);
}
