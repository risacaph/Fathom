import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

/** Mirrors Fathom.Models.DTOs.Settings.AiProviderConfigDto (camelCase over the wire). */
export interface AiProviderConfig {
  enabled: boolean;
  apiBaseUrl: string;
  apiKey: string;
  chatModel: string;
  embeddingModel: string;
  maxContextChars: number;
  temperature: number;
  isConfigured: boolean;
  canEmbed: boolean;
}

/** Lightweight capability probe — no secrets. */
export interface AiStatus {
  enabled: boolean;
  canEmbed: boolean;
  chatModel: string;
}

export interface SemanticSearchResult {
  chapterId: number;
  seriesId: number;
  libraryId: number;
  chunkIndex: number;
  snippet: string;
  score: number;
}

export interface RagAnswer {
  answer: string;
  sources: SemanticSearchResult[];
  model: string;
  hasContext: boolean;
}

export interface AiEnrichmentResult {
  seriesId: number;
  seriesName: string;
  summary: string;
  tagline: string;
  genres: string[];
  tags: string[];
  model: string;
  generatedUtc: string;
}

/**
 * Client for the AI subsystem (F6 semantic search / RAG, F8 metadata enrichment).
 * All features are inert until an admin configures a provider via {@link updateConfig}.
 */
@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /** Capability probe for any authenticated user (drives show/hide of AI UI). */
  getStatus() {
    return this.http.get<AiStatus>(this.baseUrl + 'ai/status');
  }

  /** Full provider configuration (admin only). */
  getConfig() {
    return this.http.get<AiProviderConfig>(this.baseUrl + 'ai/config');
  }

  /** Persist provider configuration (admin only). Returns the saved config. */
  updateConfig(config: AiProviderConfig) {
    return this.http.post<AiProviderConfig>(this.baseUrl + 'ai/config', config);
  }

  /** AI metadata suggestions for a series (admin only) — F8. */
  enrichSeries(seriesId: number) {
    return this.http.post<AiEnrichmentResult>(this.baseUrl + `ai/enrich/series/${seriesId}`, {});
  }

  /** Meaning-based search across the user's libraries — F6. */
  semanticSearch(query: string, limit = 10) {
    return this.http.post<SemanticSearchResult[]>(this.baseUrl + 'ai/semantic-search', { query, limit });
  }

  /** Retrieval-augmented answer grounded in the user's library — F6. */
  ask(question: string, topK = 6) {
    return this.http.post<RagAnswer>(this.baseUrl + 'ai/ask', { question, topK });
  }

  /** Kick off a background re-embed of all text libraries (admin only) — F6. */
  reindexSemantic() {
    return this.http.post(this.baseUrl + 'ai/semantic/reindex', {});
  }
}
