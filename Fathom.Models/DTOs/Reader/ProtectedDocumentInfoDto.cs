namespace Fathom.Models.DTOs.Reader;

/// <summary>
/// Lightweight description of a document exposed through the protected (DRM-lite) reader. It intentionally
/// carries no file paths or download URLs — the secure viewer only ever receives rendered, watermarked page
/// images, never the source file.
/// </summary>
public class ProtectedDocumentInfoDto
{
    public int ChapterId { get; set; }
    /// <summary>Total number of pages that can be requested (0-based indices 0..Pages-1).</summary>
    public int Pages { get; set; }
    public string Title { get; set; } = string.Empty;
}
