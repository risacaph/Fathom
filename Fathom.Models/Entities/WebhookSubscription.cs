using System;

namespace Fathom.Models.Entities;

/// <summary>
/// An outbound webhook endpoint. Fathom POSTs event payloads here (optionally HMAC-signed).
/// </summary>
public class WebhookSubscription
{
    public int Id { get; set; }

    /// <summary>Target URL that receives the POST.</summary>
    public required string Url { get; set; }

    /// <summary>
    /// Optional shared secret. When set, each payload is signed with an X-Fathom-Signature
    /// (HMAC-SHA256 hex of the body) so the receiver can verify authenticity.
    /// </summary>
    public string? Secret { get; set; }

    public bool IsEnabled { get; set; } = true;

    /// <summary>Comma-separated event names to deliver, or "*" for all events.</summary>
    public string EventFilter { get; set; } = "*";

    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
