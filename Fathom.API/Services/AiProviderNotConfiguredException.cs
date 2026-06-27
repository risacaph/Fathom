using System;

namespace Fathom.API.Services;

/// <summary>
/// Thrown when an AI operation is requested but the AI provider is disabled or not fully configured.
/// Controllers translate this into a friendly 4xx so the feature degrades gracefully.
/// </summary>
public sealed class AiProviderNotConfiguredException : Exception
{
    public AiProviderNotConfiguredException(string message) : base(message) { }
}
