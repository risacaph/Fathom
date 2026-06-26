using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities;

namespace Fathom.API.Services;

/// <summary>
/// Outbound webhooks: register endpoints and deliver (optionally HMAC-signed) event payloads to them.
/// </summary>
public interface IWebhookService
{
    Task<IList<WebhookSubscription>> GetAllAsync(CancellationToken ct = default);
    Task<WebhookSubscription> CreateAsync(WebhookSubscription sub, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> TestAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Deliver an event payload to every enabled subscription whose filter matches. Safe to call from a
    /// Hangfire job (runs in its own scope), which is how event producers should invoke it.
    /// </summary>
    Task DeliverAsync(string eventName, string payloadJson, CancellationToken ct = default);
}
