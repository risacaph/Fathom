# Advanced features F6–F10 — implementation specs

F1–F5 are implemented and verified in this branch (full-text search, DOI/CrossRef metadata,
citation export, document versioning; annotation export already shipped upstream). F6–F10 below
each depend on an **external service or toolchain** that cannot be exercised/verified in the build
sandbox (no LLM API keys, no Tesseract binary, no audio transcoder, no mobile toolchain), so they
are specified here as concrete, codebase-grounded plans + the hooks they build on. Each can be
implemented and verified once its dependency is available.

Conventions used by F1–F5 that these reuse: service in `Fathom.Services` + interface in
`Fathom.API/Services`, DI in `ApplicationServiceExtensions.AddFathomServices`, raw-SQL via
`unitOfWork.DataContext.Database`, EF migration via `dotnet ef`, endpoints on a controller in
`Fathom.Server/Controllers`, `[Authorize(Policy = PolicyGroups.AdminPolicy)]` for admin actions,
Hangfire `BackgroundJob.Enqueue` for long work.

---

## F6 — Semantic search + RAG Q&A
**Builds on:** F1's `ChapterFts` content extraction (`BookService.ExtractPlainTextAsync`).
**External dep:** an embeddings model + an LLM (Anthropic/OpenAI/Ollama) — API key or local endpoint.

1. **Chunking + embeddings store.** New table `ChapterChunkEmbedding(ChapterId, ChunkIndex, Text,
   PageNumber, Embedding BLOB)` via a raw-SQL migration (mirror `AddFullTextSearchFts5`). Reuse the
   F1 extractor, split text into ~512-token chunks, embed each, store the vector as a BLOB. Optional:
   `sqlite-vss`/`sqlite-vec` extension for ANN, else brute-force cosine in C# (fine to ~100k chunks).
2. **`IEmbeddingProvider`** (`Fathom.API/Services`) with `EmbedAsync(string[])`; impls for
   Anthropic/OpenAI/Ollama selected by a new `AiSettings` section (endpoint, key, model). Default
   impl returns "not configured".
3. **`ISemanticSearchService.SearchAsync(query, libraryIds, k)`** — embed the query, rank chunks by
   cosine, return `{chapterId, page, snippet, score}`.
4. **RAG**: `IRagService.AskAsync(question, libraryIds)` — retrieve top-k chunks, build a prompt with
   citations, call the LLM, return answer + `[{chapterId, page}]` sources.
5. **API**: `GET /api/search/semantic`, `POST /api/search/ask`. Reindex via a Hangfire job like
   `ReindexAllAsync`.
**Effort:** M–L. Verify with a stubbed provider (cosine over deterministic fake vectors) + one live key.

---

## F7 — OCR pipeline (scanned PDFs)
**Builds on:** F1 (OCR output feeds the same `ChapterFts`/embeddings).
**External dep:** Tesseract (`tesseract-ocr`) or OCRmyPDF in the container image.

1. Detect image-only PDFs: in `ExtractPlainTextAsync`, if Docnet `GetText()` yields ~0 chars over N
   pages, mark the chapter `NeedsOcr`.
2. **`IOcrService.OcrPdfAsync(filePath)`** shells out to `ocrmypdf --sidecar out.txt` (or renders
   pages via the existing Docnet image path → `tesseract`). Run as a Hangfire job (slow).
3. Store the OCR text and index it (F1) / embed it (F6). Add a `Library.EnableOcr` setting + admin
   "OCR scan" trigger.
**Effort:** M. Add `tesseract-ocr` to the Dockerfile; verify on a sample scanned PDF.

---

## F8 — AI summarize / auto-tag / translate
**Builds on:** F6's `IEmbeddingProvider`/LLM settings; writes to existing `SeriesMetadata.Summary`,
genres/tags, and `Language`.
**External dep:** LLM API (shared with F6).

1. **`IAiDocumentService`**: `SummarizeAsync(seriesId)` (→ `Summary`, respecting `SummaryLocked`),
   `SuggestTagsAsync(seriesId)` (→ genres/tags), `TranslateAsync(seriesId, lang)`.
2. On scan completion (hook in `ProcessSeries`) optionally enqueue summarize/auto-tag for
   Research/Regulations libraries, behind an `AiSettings.AutoEnrich` flag.
3. **API**: `POST /api/metadata/ai/summarize|tags|translate?seriesId=` (admin), preview-then-apply.
**Effort:** S–M once F6's provider exists. Verify with stub + one live key.

---

## F9 — Audiobooks / podcasts + format conversion
**External dep:** audio probing/transcoding (ffmpeg) for audio; Calibre (`ebook-convert`) for ebooks.

1. **Audiobooks** (Audiobookshelf-style): new `LibraryType.Audiobook`; parse `.m4b/.mp3` (chapters via
   ffprobe), a streaming endpoint with range support, and per-user audio progress (extend
   `AppUserProgress` with a position-seconds field). This is the largest item — effectively a new
   media path through scanner → reader.
2. **Format conversion / send-to-device**: `IConversionService.ConvertAsync(chapterId, target)` shells
   to Calibre `ebook-convert` (epub↔mobi↔pdf); "Send to Kindle" reuses `IEmailService`; "Send to Kobo"
   via the Kobo store-sync API (Calibre-Web-style).
**Effort:** L (audiobooks), S–M (conversion). Add ffmpeg/Calibre to the image; verify per format.

---

## F10 — Stats, webhooks, notifications, social, mobile/sync

### Tractable now (buildable + verifiable like F2–F5, no external service to *build*)
- **Webhooks**: `WebhookSubscription(Url, Events, Secret, IsEnabled)` entity + migration + CRUD
  endpoints + `IWebhookService.DispatchAsync(event, payload)` (HMAC-signed POST via Flurl). Wire
  `DispatchAsync` into existing `IEventHub` emissions. *(Recommended next real increment.)*
- **Notifications**: `INotificationService` posting to **Apprise/ntfy/Discord** webhook URLs on
  scan-complete/error; a settings section for the target URL.
- **Stats dashboards**: extend `IStatisticService` (already present) with reading-time/streaks/pages
  series + new chart endpoints; UI panel.

### Larger (own programs)
- **Social** (reviews/shelves/clubs/federation): builds on existing ratings + the social-annotations
  model; federation (ActivityPub/Bookwyrm-style) is a separate service.
- **Mobile + generic sync**: a documented public sync API (extend the existing KOReader sync) + a
  separate native app project (out of this repo).

**Effort:** webhooks/notifications/stats S–M (do these next, in-repo, verifiable); social/mobile L+.

---

### Suggested build order once dependencies are available
1. **F10 webhooks + notifications + stats** (no external build dep — pure in-repo, verifiable now).
2. **F6 embeddings/semantic + RAG** (needs an LLM key) → unlocks **F8** (summarize/tag/translate).
3. **F7 OCR** (needs Tesseract in image) → feeds F1/F6.
4. **F9 conversion** (needs Calibre), then **audiobooks** (needs ffmpeg + new media path).
5. **Social / mobile** as standalone follow-on programs.
