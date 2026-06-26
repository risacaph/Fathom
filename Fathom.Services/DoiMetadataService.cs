using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Services;
using Fathom.Common.EnvironmentInfo;
using Fathom.Models.DTOs.Metadata;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// Resolves academic metadata by DOI using the open CrossRef REST API (https://api.crossref.org).
/// No API key required; CrossRef asks callers to identify themselves via User-Agent.
/// </summary>
public partial class DoiMetadataService(ILogger<DoiMetadataService> logger) : IDoiMetadataService
{
    private const string CrossRefApi = "https://api.crossref.org/works";

    public async Task<AcademicMetadataDto?> LookupByDoiAsync(string doi, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(doi)) return null;

        doi = doi.Trim()
            .Replace("https://doi.org/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://doi.org/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("doi:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        try
        {
            var json = await $"{CrossRefApi}/{Uri.EscapeDataString(doi)}"
                .WithHeader("User-Agent", $"Fathom/{BuildInfo.Version} (https://fathomreader.com)")
                .GetStringAsync(cancellationToken: ct);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("message", out var m)) return null;

            return new AcademicMetadataDto
            {
                Doi = doi,
                Title = FirstArrayString(m, "title"),
                Authors = ParseAuthors(m),
                Year = ParseYear(m),
                Abstract = m.TryGetProperty("abstract", out var ab) ? StripMarkup(ab.GetString()) : null,
                Venue = FirstArrayString(m, "container-title"),
                Publisher = m.TryGetProperty("publisher", out var p) ? p.GetString() : null,
                Type = m.TryGetProperty("type", out var ty) ? ty.GetString() : null,
                Url = m.TryGetProperty("URL", out var u) ? u.GetString() : $"https://doi.org/{doi}"
            };
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning("[DOI] CrossRef lookup failed for {Doi} ({Status})", doi, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[DOI] Error resolving {Doi}", doi);
            return null;
        }
    }

    private static string? FirstArrayString(JsonElement m, string prop) =>
        m.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.Array && el.GetArrayLength() > 0
            ? el[0].GetString()
            : null;

    private static IList<string> ParseAuthors(JsonElement m)
    {
        var authors = new List<string>();
        if (!m.TryGetProperty("author", out var arr) || arr.ValueKind != JsonValueKind.Array) return authors;

        foreach (var a in arr.EnumerateArray())
        {
            var given = a.TryGetProperty("given", out var g) ? g.GetString() : null;
            var family = a.TryGetProperty("family", out var f) ? f.GetString() : null;
            var name = string.Join(" ", new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(name)) authors.Add(name);
        }

        return authors;
    }

    private static int? ParseYear(JsonElement m)
    {
        foreach (var key in new[] { "published-print", "published-online", "issued", "created" })
        {
            if (m.TryGetProperty(key, out var pub)
                && pub.TryGetProperty("date-parts", out var dp)
                && dp.ValueKind == JsonValueKind.Array && dp.GetArrayLength() > 0
                && dp[0].ValueKind == JsonValueKind.Array && dp[0].GetArrayLength() > 0
                && dp[0][0].TryGetInt32(out var year))
            {
                return year;
            }
        }

        return null;
    }

    private static string? StripMarkup(string? s) =>
        string.IsNullOrEmpty(s) ? s : JatsTag().Replace(s, string.Empty).Trim();

    [GeneratedRegex("<.*?>")]
    private static partial Regex JatsTag();
}
