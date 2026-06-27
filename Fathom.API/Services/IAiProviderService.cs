using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Settings;

namespace Fathom.API.Services;

/// <summary>
/// Thin client over an OpenAI-compatible AI provider (chat completions + embeddings). Shared foundation for
/// semantic search / RAG (F6) and metadata enrichment (F8). Reads its configuration from the AiConfiguration
/// server setting, so the whole subsystem is dormant until an admin enables and configures a provider.
/// </summary>
public interface IAiProviderService
{
    /// <summary>Current persisted AI provider configuration (never null; defaults when unset).</summary>
    Task<AiProviderConfigDto> GetConfigAsync(CancellationToken ct = default);

    /// <summary>Persist the AI provider configuration.</summary>
    Task SaveConfigAsync(AiProviderConfigDto config, CancellationToken ct = default);

    /// <summary>True when the provider is enabled and has the minimum fields to make chat calls.</summary>
    Task<bool> IsEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Run a single chat completion and return the assistant's text. Throws <see cref="AiProviderNotConfiguredException"/>
    /// if the provider is not enabled/configured.
    /// </summary>
    Task<string> ChatAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);

    /// <summary>
    /// Embed one or more inputs, returning a vector per input in the same order. Throws
    /// <see cref="AiProviderNotConfiguredException"/> if embeddings are not configured.
    /// </summary>
    Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct = default);
}
