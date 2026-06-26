using System.ComponentModel;

namespace Fathom.Models.Entities.Enums;

public enum LibraryType
{
    /// <summary>
    /// Uses Manga regex for filename parsing
    /// </summary>
    [Description("Manga")]
    Manga = 0,
    /// <summary>
    /// Uses Comic regex for filename parsing
    /// </summary>
    /// <remarks>This was the original implementation and is much more flexible</remarks>
    [Description("Comic (Flexible)")]
    Comic = 1,
    /// <summary>
    /// Uses Manga regex for filename parsing also uses epub metadata
    /// </summary>
    [Description("Book")]
    Book = 2,
    /// <summary>
    /// Uses a different type of grouping and parsing mechanism
    /// </summary>
    [Description("Image")]
    Image = 3,
    /// <summary>
    /// Allows Books to Scrobble with AniList for Kavita+
    /// </summary>
    [Description("Light Novel")]
    LightNovel = 4,
    /// <summary>
    /// Uses Comic regex for filename parsing, uses Comic Vine type of Parsing
    /// </summary>
    [Description("Comic")]
    ComicVine = 5,
    /// <summary>
    /// Maritime regulations / reference documents (PDF/EPUB). Parses and reads like a Book library.
    /// </summary>
    [Description("Maritime Regulations")]
    Regulations = 6,
    /// <summary>
    /// Academic research papers / theses (PDF/EPUB). Parses and reads like a Book library.
    /// </summary>
    [Description("Research Papers")]
    Research = 7,
}
