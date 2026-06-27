using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.DTOs.Settings;
using Fathom.Models.Entities.Enums;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// OpenAI-compatible AI client (chat completions + embeddings) used by semantic search/RAG (F6) and
/// metadata enrichment (F8). Configuration lives in the AiConfiguration server setting; the subsystem is
/// inert until an admin enables it. Works against any provider that speaks the OpenAI REST shape
/// (OpenAI, Azure OpenAI, OpenRouter, Ollama, LM Studio, LocalAI, …).
/// </summary>
public class AiProviderService(IUnitOfWork unitOfWork, ILogger<AiProviderService> logger) : IAiProviderService
{
    public async Task<AiProviderConfigDto> GetConfigAsync(CancellationToken ct = default)
    {
        var setting = await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.AiConfiguration, ct);
        if (string.IsNullOrWhiteSpace(setting?.Value)) return new AiProviderConfigDto();
        try
        {
            return JsonSerializer.Deserialize<AiProviderConfigDto>(setting.Value) ?? new AiProviderConfigDto();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "[AI] Stored AiConfiguration is not valid JSON; using defaults");
            return new AiProviderConfigDto();
        }
    }

    public async Task SaveConfigAsync(AiProviderConfigDto config, CancellationToken ct = default)
    {
        var setting = await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.AiConfiguration, ct);
        if (setting == null) return;
        setting.Value = JsonSerializer.Serialize(config);
        unitOfWork.SettingsRepository.Update(setting);
        await unitOfWork.CommitAsync();
    }

    public async Task<bool> IsEnabledAsync(CancellationToken ct = default) =>
        (await GetConfigAsync(ct)).IsConfigured;

    public async Task<string> ChatAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var config = await GetConfigAsync(ct);
        if (!config.IsConfigured)
            throw new AiProviderNotConfiguredException("AI provider is not enabled or configured.");

        try
        {
            var response = await BuildRequest(config, "chat/completions")
                .PostJsonAsync(new
                {
                    model = config.ChatModel,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = config.Temperature
                }, cancellationToken: ct)
                .ReceiveJson<ChatCompletionResponse>();

            return response?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
        }
        catch (FlurlHttpException ex)
        {
            var body = await SafeBody(ex);
            logger.LogError(ex, "[AI] Chat completion failed ({Status}): {Body}", ex.StatusCode, body);
            throw new InvalidOperationException($"AI chat request failed ({ex.StatusCode}). {body}", ex);
        }
    }

    public async Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct = default)
    {
        if (inputs.Count == 0) return Array.Empty<float[]>();

        var config = await GetConfigAsync(ct);
        if (!config.CanEmbed)
            throw new AiProviderNotConfiguredException("AI provider embeddings are not enabled or configured.");

        try
        {
            var response = await BuildRequest(config, "embeddings")
                .PostJsonAsync(new { model = config.EmbeddingModel, input = inputs }, cancellationToken: ct)
                .ReceiveJson<EmbeddingResponse>();

            return response?.Data?
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding ?? Array.Empty<float>())
                .ToList() ?? new List<float[]>();
        }
        catch (FlurlHttpException ex)
        {
            var body = await SafeBody(ex);
            logger.LogError(ex, "[AI] Embeddings request failed ({Status}): {Body}", ex.StatusCode, body);
            throw new InvalidOperationException($"AI embeddings request failed ({ex.StatusCode}). {body}", ex);
        }
    }

    private static IFlurlRequest BuildRequest(AiProviderConfigDto config, string path)
    {
        var request = config.ApiBaseUrl
            .AppendPathSegment(path)
            .WithHeader("Accept", "application/json");
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            request = request.WithHeader("Authorization", "Bearer " + config.ApiKey);
        return request;
    }

    private static async Task<string> SafeBody(FlurlHttpException ex)
    {
        try { return await ex.GetResponseStringAsync(); }
        catch { return string.Empty; }
    }

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")] public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; set; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")] public List<EmbeddingData>? Data { get; set; }
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("embedding")] public float[]? Embedding { get; set; }
    }
}
