using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Fathom.Viewer;

/// <summary>
/// Talks to a Fathom server. Authenticates with the user's credentials (JWT bearer) and pulls per-page,
/// server-watermarked images from the protected-reader API. It never requests or stores the source file.
/// </summary>
public sealed class FathomClient
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };

    public string BaseUrl { get; private set; } = "";
    public string? Username { get; private set; }
    public bool IsAuthenticated => _http.DefaultRequestHeaders.Authorization != null;

    public async Task LoginAsync(string baseUrl, string username, string password, CancellationToken ct = default)
    {
        BaseUrl = baseUrl.Trim().TrimEnd('/');
        using var resp = await _http.PostAsJsonAsync($"{BaseUrl}/api/account/login",
            new { username, password }, ct);
        resp.EnsureSuccessStatusCode();

        var user = await resp.Content.ReadFromJsonAsync<UserDto>(ct);
        if (string.IsNullOrEmpty(user?.Token))
            throw new InvalidOperationException("Login succeeded but no token was returned.");

        Username = user.Username;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
    }

    public async Task<List<SearchSeries>> SearchAsync(string query, CancellationToken ct = default)
    {
        var group = await _http.GetFromJsonAsync<SearchGroup>(
            $"{BaseUrl}/api/search/search?queryString={Uri.EscapeDataString(query)}", ct);
        return group?.Series ?? new List<SearchSeries>();
    }

    public async Task<List<ChapterItem>> GetChaptersAsync(int seriesId, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<List<ChapterItem>>(
            $"{BaseUrl}/api/search/chapters-by-series?seriesId={seriesId}", ct) ?? new List<ChapterItem>();

    public async Task<DocInfo> GetInfoAsync(int chapterId, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<DocInfo>(
            $"{BaseUrl}/api/protectedreader/info?chapterId={chapterId}", ct) ?? new DocInfo();

    /// <summary>Fetch one rendered, watermarked page. Bytes are held in memory only by the caller.</summary>
    public Task<byte[]> GetPageAsync(int chapterId, int page, CancellationToken ct = default) =>
        _http.GetByteArrayAsync($"{BaseUrl}/api/protectedreader/page?chapterId={chapterId}&page={page}", ct);
}

// Wire DTOs. HttpClient JSON helpers use Web defaults (camelCase, case-insensitive), so these map directly.
public sealed record UserDto
{
    public string? Token { get; set; }
    public string? Username { get; set; }
}

public sealed record SearchGroup
{
    public List<SearchSeries> Series { get; set; } = new();
}

public sealed record SearchSeries
{
    public int SeriesId { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}

public sealed record ChapterItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Range { get; set; }

    public string Display =>
        !string.IsNullOrWhiteSpace(Title) ? Title!
        : !string.IsNullOrWhiteSpace(Range) ? $"Chapter {Range}"
        : $"Chapter {Id}";

    public override string ToString() => Display;
}

public sealed record DocInfo
{
    public int ChapterId { get; set; }
    public int Pages { get; set; }
    public string? Title { get; set; }
}
