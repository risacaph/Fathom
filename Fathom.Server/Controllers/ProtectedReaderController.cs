using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Services;
using Fathom.Models.DTOs.Reader;
using Fathom.Server.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

/// <summary>
/// Protected (DRM-lite) document viewer API. Serves per-user watermarked page images so the source file
/// never leaves the server. Consumed by the Fathom secure desktop viewer (Windows MSI), which additionally
/// blocks screen capture and disables copy/print/save on the client. Access is gated by the authenticated
/// user's normal library/series permissions via <see cref="ChapterAccessAttribute"/>.
/// </summary>
public class ProtectedReaderController(IProtectedReaderService protectedReaderService) : BaseApiController
{
    /// <summary>Page count + title for a document the authenticated caller is permitted to read.</summary>
    [ChapterAccess]
    [HttpGet("info")]
    public async Task<ActionResult<ProtectedDocumentInfoDto>> GetInfo(int chapterId, CancellationToken ct)
    {
        try
        {
            return Ok(await protectedReaderService.GetInfoAsync(chapterId, ct));
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// A single page rendered as a per-user watermarked JPEG. Marked no-store so intermediaries won't cache
    /// it; the secure viewer holds it in memory only and never writes the source file to disk.
    /// </summary>
    [ChapterAccess]
    [HttpGet("page")]
    public async Task<ActionResult> GetPage(int chapterId, int page, CancellationToken ct)
    {
        try
        {
            var bytes = await protectedReaderService.RenderWatermarkedPageAsync(chapterId, page, BuildWatermark(), ct);
            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, private";
            Response.Headers.Pragma = "no-cache";
            return File(bytes, "image/jpeg");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Identifies the viewer in the watermark so any leaked screen photo is traceable.</summary>
    private string BuildWatermark()
    {
        var who = UserContext.GetUsername() ?? $"user {UserId}";
        return $"{who} • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
    }
}
