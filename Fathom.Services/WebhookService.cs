using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.Entities;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fathom.Services;

/// <summary>
/// Stores webhook subscriptions and delivers event payloads to them (optionally HMAC-SHA256 signed).
/// </summary>
public class WebhookService(IUnitOfWork unitOfWork, ILogger<WebhookService> logger) : IWebhookService
{
    public async Task<IList<WebhookSubscription>> GetAllAsync(CancellationToken ct = default) =>
        await unitOfWork.DataContext.WebhookSubscription.ToListAsync(ct);

    public async Task<WebhookSubscription> CreateAsync(WebhookSubscription sub, CancellationToken ct = default)
    {
        sub.CreatedUtc = DateTime.UtcNow;
        sub.LastModifiedUtc = DateTime.UtcNow;
        unitOfWork.DataContext.WebhookSubscription.Add(sub);
        await unitOfWork.CommitAsync();
        return sub;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var sub = await unitOfWork.DataContext.WebhookSubscription.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sub == null) return false;
        unitOfWork.DataContext.WebhookSubscription.Remove(sub);
        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> TestAsync(int id, CancellationToken ct = default)
    {
        var sub = await unitOfWork.DataContext.WebhookSubscription.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sub == null) return false;
        await PostAsync(sub, "test", "{\"event\":\"test\",\"message\":\"Fathom webhook test\"}", ct);
        return true;
    }

    public async Task DeliverAsync(string eventName, string payloadJson, CancellationToken ct = default)
    {
        var subs = await unitOfWork.DataContext.WebhookSubscription.Where(s => s.IsEnabled).ToListAsync(ct);
        foreach (var sub in subs.Where(s => Matches(s.EventFilter, eventName)))
        {
            try
            {
                await PostAsync(sub, eventName, payloadJson, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Webhook] Delivery of {Event} to {Url} failed", eventName, sub.Url);
            }
        }
    }

    private static bool Matches(string filter, string eventName) =>
        string.IsNullOrWhiteSpace(filter) || filter.Trim() == "*" ||
        filter.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(eventName, StringComparer.OrdinalIgnoreCase);

    private static async Task PostAsync(WebhookSubscription sub, string eventName, string payloadJson, CancellationToken ct)
    {
        var req = sub.Url
            .WithHeader("Content-Type", "application/json")
            .WithHeader("X-Fathom-Event", eventName);

        if (!string.IsNullOrEmpty(sub.Secret))
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sub.Secret));
            var sig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson))).ToLowerInvariant();
            req = req.WithHeader("X-Fathom-Signature", "sha256=" + sig);
        }

        await req.PostStringAsync(payloadJson, cancellationToken: ct);
    }
}
