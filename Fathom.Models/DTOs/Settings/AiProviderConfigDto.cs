#nullable enable

namespace Fathom.Models.DTOs.Settings;

/// <summary>
/// Configuration for the optional AI provider that powers semantic search / RAG (F6) and
/// AI metadata enrichment (F8). Targets any OpenAI-compatible Chat Completions + Embeddings API
/// (OpenAI, Azure OpenAI, OpenRouter, Ollama, LM Studio, LocalAI, …), so an admin can point it at
/// a hosted provider or a self-hosted local model.
/// </summary>
/// <remarks>Saved as a JSON object in a single ServerSetting row (like OIDC); default values prevent NPEs.</remarks>
public sealed record AiProviderConfigDto
{
    /// <summary>Master switch. When false, all AI features are disabled regardless of the other fields.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Base URL of the OpenAI-compatible API, including any version segment.
    /// Examples: "https://api.openai.com/v1", "https://openrouter.ai/api/v1", "http://localhost:11434/v1" (Ollama).
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>API key / bearer token. Optional for keyless local providers (e.g. Ollama).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Chat/completions model id used for RAG answers and metadata enrichment.</summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>Embeddings model id used to index and query document chunks.</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>Upper bound on characters of retrieved context fed into a RAG prompt.</summary>
    public int MaxContextChars { get; set; } = 12000;

    /// <summary>Sampling temperature for chat completions (0 = deterministic).</summary>
    public double Temperature { get; set; } = 0.2;

    /// <summary>True when the feature is switched on and the minimum fields are present to make a call.</summary>
    public bool IsConfigured =>
        Enabled && !string.IsNullOrWhiteSpace(ApiBaseUrl) && !string.IsNullOrWhiteSpace(ChatModel);

    /// <summary>True when embeddings (semantic search) can be performed.</summary>
    public bool CanEmbed => IsConfigured && !string.IsNullOrWhiteSpace(EmbeddingModel);
}
