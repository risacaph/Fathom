using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Repositories;
using Fathom.API.Services;
using Fathom.Models.Constants;
using Fathom.Models.DTOs.Metadata;
using Fathom.Models.DTOs.Search;
using Fathom.Models.DTOs.Settings;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fathom.Server.Controllers;

/// <summary>
/// AI features: provider configuration, semantic search / retrieval-augmented Q&amp;A (F6) and metadata
/// enrichment (F8). Everything here is inert until an admin enables and configures an OpenAI-compatible
/// provider via <c>POST api/ai/config</c>.
/// </summary>
public class AiController(IUnitOfWork unitOfWork, IAiProviderService aiProvider,
    IAiMetadataService aiMetadata, ISemanticSearchService semanticSearch) : BaseApiController
{
    /// <summary>
    /// Lightweight capability probe for any authenticated user, so the UI can show/hide AI features without
    /// exposing the provider credentials.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetStatus(CancellationToken ct)
    {
        var config = await aiProvider.GetConfigAsync(ct);
        return Ok(new { enabled = config.IsConfigured, canEmbed = config.CanEmbed, chatModel = config.ChatModel });
    }

    /// <summary>Get the AI provider configuration (admin).</summary>
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    [HttpGet("config")]
    public async Task<ActionResult<AiProviderConfigDto>> GetConfig(CancellationToken ct)
    {
        return Ok(await aiProvider.GetConfigAsync(ct));
    }

    /// <summary>Update the AI provider configuration (admin).</summary>
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    [HttpPost("config")]
    public async Task<ActionResult<AiProviderConfigDto>> SaveConfig([FromBody] AiProviderConfigDto config, CancellationToken ct)
    {
        await aiProvider.SaveConfigAsync(config, ct);
        return Ok(await aiProvider.GetConfigAsync(ct));
    }

    /// <summary>Generate AI metadata suggestions (summary/tagline/genres/tags) for a series (admin) — F8.</summary>
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    [HttpPost("enrich/series/{seriesId:int}")]
    public async Task<ActionResult<AiEnrichmentResultDto>> EnrichSeries(int seriesId, CancellationToken ct)
    {
        try
        {
            return Ok(await aiMetadata.EnrichSeriesAsync(seriesId, ct));
        }
        catch (AiProviderNotConfiguredException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Semantic (meaning-based) search inside book/PDF text, scoped to the user's libraries — F6.</summary>
    [HttpPost("semantic-search")]
    public async Task<ActionResult<IList<SemanticSearchResultDto>>> SemanticSearch([FromBody] SemanticSearchRequestDto request, CancellationToken ct)
    {
        var libraries = await unitOfWork.LibraryRepository.GetLibraryIdsForUserIdAsync(UserId, QueryContext.Search);
        if (libraries.Count == 0) return Ok(new List<SemanticSearchResultDto>());

        try
        {
            return Ok(await semanticSearch.SearchAsync(request.Query, libraries, request.Limit, ct));
        }
        catch (AiProviderNotConfiguredException ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Ask a natural-language question answered from the user's own library (RAG) — F6.</summary>
    [HttpPost("ask")]
    public async Task<ActionResult<RagAnswerDto>> Ask([FromBody] RagRequestDto request, CancellationToken ct)
    {
        var libraries = await unitOfWork.LibraryRepository.GetLibraryIdsForUserIdAsync(UserId, QueryContext.Search);
        if (libraries.Count == 0) return Ok(new RagAnswerDto());

        try
        {
            return Ok(await semanticSearch.AskAsync(request.Question, libraries, request.TopK, ct));
        }
        catch (AiProviderNotConfiguredException ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Rebuild the semantic index for all text-based libraries (admin; runs in the background) — F6.</summary>
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    [HttpPost("semantic/reindex")]
    public ActionResult ReindexSemantic()
    {
        BackgroundJob.Enqueue<ISemanticSearchService>(s => s.ReindexAllAsync(default));
        return Ok();
    }
}
