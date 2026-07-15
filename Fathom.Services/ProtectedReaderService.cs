using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Services;
using Fathom.Models.DTOs.Reader;
using Microsoft.Extensions.Logging;
using NetVips;

namespace Fathom.Services;

/// <summary>
/// Renders document pages as per-user watermarked images for the protected (DRM-lite) viewer. The source
/// file is never returned; the client only receives rendered JPEGs. Watermarking is best-effort — if libvips
/// text rendering is unavailable at runtime, the raw page is served (still gated behind auth + the client's
/// screen-capture blocking) rather than failing the request.
/// </summary>
public class ProtectedReaderService(ICacheService cacheService, ILogger<ProtectedReaderService> logger)
    : IProtectedReaderService
{
    public async Task<ProtectedDocumentInfoDto> GetInfoAsync(int chapterId, CancellationToken ct = default)
    {
        var chapter = await cacheService.Ensure(chapterId, true, ct);
        if (chapter == null) throw new FileNotFoundException($"Chapter {chapterId} not found");

        return new ProtectedDocumentInfoDto
        {
            ChapterId = chapter.Id,
            Pages = chapter.Pages,
            Title = chapter.TitleName ?? string.Empty
        };
    }

    public async Task<byte[]> RenderWatermarkedPageAsync(int chapterId, int page, string watermark,
        CancellationToken ct = default)
    {
        if (page < 0) page = 0;

        var chapter = await cacheService.Ensure(chapterId, true, ct);
        if (chapter == null) throw new FileNotFoundException($"Chapter {chapterId} not found");

        var path = cacheService.GetCachedPagePath(chapter.Id, page);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            throw new FileNotFoundException($"Page {page} not found for chapter {chapterId}");

        try
        {
            return Watermark(path, watermark);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[ProtectedReader] Watermarking failed for chapter {ChapterId} page {Page}; serving raw page",
                chapterId, page);
            return await File.ReadAllBytesAsync(path, ct);
        }
    }

    /// <summary>
    /// Composite a low-opacity, tiled watermark across the page using libvips and re-encode as JPEG.
    /// </summary>
    private static byte[] Watermark(string path, string text)
    {
        using var source = Image.NewFromFile(path);

        // Normalise to an opaque, distinct sRGB image (Copy so both `source` and `opaque` are disposable).
        using var opaque = source.HasAlpha() ? source.Flatten() : source.Copy();
        using var rgb = opaque.Colourspace(Enums.Interpretation.Srgb);

        // Render the watermark text to a coverage mask (single band, 0..255).
        using var mask = Image.Text($"   {text}   ", dpi: 96, font: "sans 22");

        // Build a translucent white RGBA tile from the mask.
        using var white = mask.NewFromImage(255.0, 255.0, 255.0);       // 3-band white, mask-sized
        using var alpha = (mask * 0.12).Cast(Enums.BandFormat.Uchar);   // ~12% opacity
        using var tile = white.Bandjoin(alpha);                         // RGBA

        // Tile across the page then crop back to the page dimensions.
        var across = (int)Math.Ceiling((double)rgb.Width / Math.Max(1, tile.Width)) + 1;
        var down = (int)Math.Ceiling((double)rgb.Height / Math.Max(1, tile.Height)) + 1;
        using var replicated = tile.Replicate(across, down);
        using var overlay = replicated.Crop(0, 0, rgb.Width, rgb.Height);

        // Composite and flatten to a JPEG buffer.
        using var composited = rgb.Composite2(overlay, Enums.BlendMode.Over);
        using var final = composited.HasAlpha() ? composited.Flatten() : composited.Copy();
        return final.JpegsaveBuffer(q: 85);
    }
}
