using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Repositories;
using Fathom.API.Services;
using Fathom.Models.DTOs.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// AI metadata enrichment (F8). Builds a prompt from a series' title and a sample of its text, asks the
/// configured chat model for strict-JSON suggestions, and returns them for admin review.
/// </summary>
public class AiMetadataService(IUnitOfWork unitOfWork, IAiProviderService ai, IBookService bookService,
    ILogger<AiMetadataService> logger) : IAiMetadataService
{
    private const int MaxSampleChars = 6000;

    private const string SystemPrompt =
        "You are a meticulous librarian's cataloguing assistant. Given a document's title and an excerpt, " +
        "you produce concise, neutral catalogue metadata. Respond with STRICT JSON only — no markdown, no code " +
        "fences, no commentary. Schema: {\"summary\": string (2-4 sentences), \"tagline\": string (one sentence), " +
        "\"genres\": string[] (1-5 items), \"tags\": string[] (3-10 keywords)}.";

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<AiEnrichmentResultDto> EnrichSeriesAsync(int seriesId, CancellationToken ct = default)
    {
        var series = await unitOfWork.SeriesRepository.GetSeriesByIdAsync(seriesId,
            SeriesIncludes.Metadata | SeriesIncludes.Library, ct);
        if (series == null) throw new KeyNotFoundException($"Series {seriesId} not found");

        var filePath = await unitOfWork.DataContext.Chapter
            .Where(c => c.Volume.SeriesId == seriesId)
            .OrderBy(c => c.Id)
            .Select(c => c.Files.Select(f => f.FilePath).FirstOrDefault())
            .FirstOrDefaultAsync(ct);

        var sample = string.Empty;
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                var text = await bookService.ExtractPlainTextAsync(filePath, ct);
                sample = Truncate(text, MaxSampleChars);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[AI] Could not extract sample text for series {SeriesId}", seriesId);
            }
        }

        var config = await ai.GetConfigAsync(ct);
        var raw = await ai.ChatAsync(SystemPrompt, BuildUserPrompt(series.Name, series.Library?.Type.ToString(),
            series.Metadata?.Summary, sample), ct);

        var parsed = Parse(raw);
        return new AiEnrichmentResultDto
        {
            SeriesId = seriesId,
            SeriesName = series.Name,
            Summary = parsed.Summary?.Trim() ?? string.Empty,
            Tagline = parsed.Tagline?.Trim() ?? string.Empty,
            Genres = Clean(parsed.Genres),
            Tags = Clean(parsed.Tags),
            Model = config.ChatModel,
            GeneratedUtc = DateTime.UtcNow
        };
    }

    private static string BuildUserPrompt(string name, string? libraryType, string? existingSummary, string sample)
    {
        var parts = new List<string> { $"Title: {name}" };
        if (!string.IsNullOrWhiteSpace(libraryType)) parts.Add($"Collection type: {libraryType}");
        if (!string.IsNullOrWhiteSpace(existingSummary)) parts.Add($"Existing summary (may be empty/poor): {existingSummary}");
        parts.Add(string.IsNullOrWhiteSpace(sample)
            ? "No text excerpt is available; infer from the title only."
            : $"Excerpt:\n\"\"\"\n{sample}\n\"\"\"");
        return string.Join("\n\n", parts);
    }

    private EnrichmentJson Parse(string raw)
    {
        try
        {
            return JsonSerializer.Deserialize<EnrichmentJson>(ExtractJson(raw), JsonOpts) ?? new EnrichmentJson();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "[AI] Enrichment response was not valid JSON: {Raw}", Truncate(raw, 500));
            return new EnrichmentJson { Summary = raw.Trim() };
        }
    }

    /// <summary>Pull the first {...} object out of a model response, tolerating fences or stray prose.</summary>
    private static string ExtractJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw.Substring(start, end - start + 1) : "{}";
    }

    private static List<string> Clean(List<string>? items) => items == null
        ? []
        : items.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct().ToList();

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value ?? string.Empty : value[..max];

    private sealed class EnrichmentJson
    {
        [JsonPropertyName("summary")] public string? Summary { get; set; }
        [JsonPropertyName("tagline")] public string? Tagline { get; set; }
        [JsonPropertyName("genres")] public List<string>? Genres { get; set; }
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
    }
}
