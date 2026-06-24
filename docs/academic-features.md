# Research-paper / academic features — plan

Fathom now ships a **Research Papers** library type (`LibraryType.Research = 7`) — the container for college
and doctoral theses/papers (PDF/EPUB), behaving like a Book library. The high-value academic features below
need the **.NET SDK** (blocked in the rebrand env: `dot.net` → 403) for new DB columns + EF migrations +
backend libraries, so they're specced here to run in a .NET 10 session. Pure-frontend items are flagged.

## 1. Full-text search *inside* documents ⭐ (highest value)
Search the text of papers, not just titles/metadata.
- Extract text per page with **PdfPig** (PDF) + the existing EPUB pipeline; store in a **SQLite FTS5** virtual
  table (e.g. `ChapterText(chapterId, page, content)`).
- Index during scan (hook `Kavita.Services/Scanner` after parse) + a one-time backfill.
- API: `GET /api/search/fulltext?q=…` → ranked snippets (FTS5 `snippet()`), grouped by series/chapter/page.
- Frontend: results panel + deep-link into the PDF reader (`ngx-extended-pdf-viewer` supports `#page=N`).
- Effort **L**. Touches scanner, `Kavita.Database` (FTS migration), a new `SearchController`, UI.

## 2. Academic metadata — DOI, authors, abstract, venue, year, keywords ⭐
Real paper metadata beyond Kavita's series fields.
- New columns on `SeriesMetadata` (or a 1:1 `ResearchMetadata` table): `Doi`, `Abstract`, `Venue`/`Journal`,
  `PublishedYear`, `Keywords`, `PublicationType` (thesis / journal / conference / preprint); reuse People
  (role **Author**) for authors. Each field with the existing `*Locked` pattern. **Needs an EF migration.**
- Auto-fill: read the PDF Info/XMP dict + regex a **DOI** (`10\.\d{4,9}/\S+`) from page 1; optional **CrossRef**
  lookup by DOI to fill authors/venue/year (if network policy allows).
- DTOs + AutoMapper + `SeriesService.UpdateSeriesMetadata`; show fields in the UI for `Research` libraries.
- Effort **M–L**. Same shape as the maritime IMO field — see `docs/phase3-maritime-library.md`.

## 3. Citation export — BibTeX / RIS / APA / MLA
- Generate from #2's metadata. Backend `GET /api/series/{id}/citation?format=bibtex|ris|apa|mla`, **or**
  client-side (frontend) from already-loaded metadata: a **"Cite"** button on series-detail + copy-to-clipboard.
- The client-side version is buildable now from existing fields (title, Writer people, release year, publisher).
- Effort **S–M**.

## 4. OCR for scanned theses
- Many scans are image-only. Run **Tesseract** (a .NET wrapper) during scan to feed #1. Effort **L**, optional.

## Frontend-buildable now (no .NET)
- **Study pack — download-for-offline + offline progress queue** (extends the PWA): pin series/libraries for
  guaranteed offline; queue reading-progress writes while offline and sync on reconnect (Background Sync or a
  retry queue). Effort **M–L** (needs offline runtime testing in a browser).
- **Data-saver mode** (localStorage pref): defer covers, request smaller images. Effort **S–M**.
- **"Cite" button** (client-side) from existing metadata. Effort **S**.
- **Reader polish**: persistent night/sepia, two-page spread, remember zoom. Effort **S–M**.

## Recommended order
1 (full-text search) and 2 (academic metadata) are the differentiators for a research-paper host; 3 (citations)
is a fast follow once 2 lands. Frontend "study pack" + data-saver can proceed in parallel without .NET.
