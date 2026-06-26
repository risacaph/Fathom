using System.Collections.Generic;
using Fathom.Models.DTOs.Misc;

namespace Fathom.Models.DTOs.ReadingLists.CBL;

public sealed record CblRepoBrowseResultDto
{
    public IList<CblRepoItemDto> Items { get; set; } = [];
    public GithubRateLimitDto RateLimitDto { get; set; } = new();
    /// <summary>
    /// True if this result was served from cache (no GitHub API call made)
    /// </summary>
    public bool FromCache { get; set; }
}
