using System.Collections.Generic;
using System.Threading.Tasks;
using Fathom.API.Services;
using Fathom.Models.Constants;
using Fathom.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

/// <summary>
/// Manage outbound webhook subscriptions (admin only).
/// </summary>
[Authorize(Policy = PolicyGroups.AdminPolicy)]
public class WebhookController(IWebhookService webhookService) : BaseApiController
{
    /// <summary>List all webhook subscriptions.</summary>
    [HttpGet]
    public async Task<ActionResult<IList<WebhookSubscription>>> GetAll() => Ok(await webhookService.GetAllAsync());

    /// <summary>Create a webhook subscription.</summary>
    [HttpPost]
    public async Task<ActionResult<WebhookSubscription>> Create(WebhookSubscription sub) => Ok(await webhookService.CreateAsync(sub));

    /// <summary>Delete a webhook subscription.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id) => await webhookService.DeleteAsync(id) ? Ok() : NotFound();

    /// <summary>Fire a test payload at a subscription.</summary>
    [HttpPost("{id:int}/test")]
    public async Task<ActionResult> Test(int id) => await webhookService.TestAsync(id) ? Ok() : NotFound();
}
