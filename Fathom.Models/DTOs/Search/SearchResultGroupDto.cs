using System.Collections.Generic;
using Fathom.Models.DTOs.Collection;
using Fathom.Models.DTOs.Metadata;
using Fathom.Models.DTOs.Person;
using Fathom.Models.DTOs.Reader;
using Fathom.Models.DTOs.ReadingLists;

namespace Fathom.Models.DTOs.Search;

/// <summary>
/// Represents all Search results for a query
/// </summary>
public sealed record SearchResultGroupDto
{
    public IEnumerable<LibraryDto> Libraries { get; set; } = default!;
    public IEnumerable<SearchResultDto> Series { get; set; } = default!;
    public IEnumerable<AppUserCollectionDto> Collections { get; set; } = default!;
    public IEnumerable<ReadingListDto> ReadingLists { get; set; } = default!;
    public IEnumerable<PersonDto> Persons { get; set; } = default!;
    public IEnumerable<GenreTagDto> Genres { get; set; } = default!;
    public IEnumerable<TagDto> Tags { get; set; } = default!;
    public IEnumerable<MangaFileDto> Files { get; set; } = default!;
    public IEnumerable<ChapterDto> Chapters { get; set; } = default!;
    public IEnumerable<BookmarkSearchResultDto> Bookmarks { get; set; } = default!;
    public IEnumerable<AnnotationDto> Annotations { get; set; } = default!;


}
