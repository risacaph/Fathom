# Fathom — Rebrand Changelog

**Fathom** ("Your library. As deep as you need it.") is a fork of **Kavita**
(<https://github.com/Kareadita/Kavita>) created by **majora2007**, licensed under **GPL-3.0**.
This file records every change made on top of upstream Kavita as part of the rebrand. It does
**not** replace upstream's authorship or attribution — see `LICENSE` and the README NOTICE.

**Fork strategy:** _tracking fork_ — we intend to keep merging upstream Kavita. Brand changes are
deliberately kept surface-level and merge-friendly. Namespaces (`Kavita.*` / `Kavita.API.*`),
project/assembly names, the database filename (`kavita.db`), the npm package name (`kavita-webui`),
and other internal identifiers are intentionally left as upstream to minimize merge conflicts.

**Decisions locked (Phase 1):** (A) tracking fork · veneer-only Kavita+ removal · telemetry disabled.

> ⚠️ The rebrand environment has **no .NET SDK**, so backend (C#) changes are string/guard-only and
> were **not compiled here**. They are syntactically safe but unverified by build. The Angular UI
> **is** buildable and is verified at the end of Phase 2.

---

## Phase 2 · Commit 1 — Disable telemetry & strip commercial/donation surfaces

### Telemetry / phone-home — disabled
- `Kavita.Services/StatsService.cs` — added a hard kill-switch `StatReportingEnabled = false`
  guarding `Send`, `SendDataToStatsServer`, and `SendCancellation`. No usage data is sent to
  `stats.kavitareader.com` (or any stats endpoint), independent of the admin toggle.
- `Kavita.Database/Seed.cs` — `AllowStatCollection` now seeds to `false` (was `true`); new installs
  are opt-out by default too.

### Commercial / donation surfaces — removed
- `UI/Web/src/environments/environment{,.prod,.proxy}.ts` — emptied `buyLink` / `manageLink`
  (removed Stripe checkout/billing URLs and the `FIRSTTIME` / `FREETRIAL` promo codes). Fields kept
  (set to `''`) so dependent components still compile.
- `UI/Web/src/app/admin/manage-system/manage-system.component.html` — removed the OpenCollective
  "donations" row plus the upstream-specific Discord / Weblate / wiki / feature-request rows;
  repointed home-page → `fathomreader.com`, source → this fork.
- Deleted `FUNDING.yml` and `.github/FUNDING.yml` (PayPal / OpenCollective sponsor config).

### Outbound identity
- `Kavita.Services/Extensions/FlurlGithubExtensions.cs` — GitHub API `User-Agent` `"Kavita"` → `"Fathom"`.

> Per the **veneer-only** decision, the Kavita+ scrobbling/metadata feature code remains in place but
> dormant — it needs a license and the upstream `plus.kavitareader.com` API, which Fathom never calls.
> The visible buy/upsell funnel is neutralized above; deeper removal is deferred to an optional Phase 3.

---

## Phase 2 · Commit 2 — Visual assets (placeholder marks)

Generated clean, antialiased **placeholder** marks (deep-navy rounded square, signal-teal "F",
brass depth line) — swap in your real logo any time by replacing these files (and re-running the
generator or deleting it).

- Added `scripts/gen-placeholder-icons.mjs` — dependency-free Node PNG/ICO generator (built-in
  `zlib` only), since the rebrand environment has no rasterizer (`sharp`/ImageMagick).
- Regenerated PNG/ICO icons: `UI/Web/src/assets/icons/{favicon-16x16,favicon-32x32,apple-touch-icon,android-chrome-192x192,android-chrome-256x256,mstile-150x150}.png`, `favicon.ico`, and the root `favicon.ico` (used by `Kavita.Server.csproj` `ApplicationIcon`).
- Regenerated `UI/Web/src/assets/images/{logo-32,logo-64,logo}.png`.
- Replaced vector logos: `UI/Web/src/assets/images/logo.svg`; added `Logo/fathom.svg`; removed `Logo/kavita.svg`.

> Left untouched (swap when you have final art): `UI/Web/src/assets/images/{logo.ai,kavita-book-cropped.png}`
> (binary source art), and the JetBrains/Sentry tooling SVGs in `Logo/` (OSS-tooling acknowledgments).

---

## Phase 2 · Commit 3 — Theme palette (deep navy / brass gold / signal teal)

Recolored the brand accent from Kavita green (`#4ac694`) to the Fathom palette:
- `theme/themes/dark.scss` — `--primary-color` ramp → signal teal (`#1AC7BC` / `#15A49B` / `#107E77` / `#0A5853`); added `--brand-navy` / `--brand-teal` / `--brand-brass` tokens; `--theme-color` → deep navy `#0B1F3A`; `--audit-log-metadata-color` → teal.
- `theme/_variables.scss` — Bootstrap `primary` fallback → teal.
- `app/shared/circular-loader/circular-loader.component.ts` — default outer stroke → teal.
- `assets/icons/browserconfig.xml`, `index.html` (`msapplication-TileColor`, `theme-color`), `site.webmanifest` (`theme_color` / `background_color`) → deep navy.

> The app's neutral dark-gray surfaces (`--bs-body-bg`, colorscape defaults) were left as upstream;
> a full navy re-tint of backgrounds is an optional design follow-up.

---

## Phase 2 · Commit 4 — Backend & email brand strings

- `Kavita.Server/Startup.cs` — Swagger Title/Description → Fathom; License URL → canonical GPL-3.0 (`gnu.org`); startup log line → `"Fathom - v…"`.
- OPDS — `Feed.cs` author Name/Uri → Fathom / `fathomreader.com`; `OpenSearchDescription.cs` Developer → `fathomreader.com`; `OpdsService.cs` root feed title → Fathom.
- Assembly metadata — `Kavita.Server.csproj` & `Kavita.Common.csproj` `Product` → Fathom, `Company` → `fathomreader.com`. **The `Copyright` line now credits both "Kavita contributors (majora2007)" and Fathom — upstream attribution preserved, never replaced.**
- Email templates — `EmailTemplates/base.html` + 8 `config/templates/*.html`: replaced the author-CDN logo image with a text wordmark, rebranded visible text (`Kavita` → `Fathom`, kept `Kavita+`), and removed/repointed author Discord/Reddit/Wiki/GitHub/OpenCollective links.

> The email hero/social icons previously hot-linked `www.kavitareader.com/img/email/*`; those `<img>`s were
> removed (we don't rehost them) — drop in your own imagery later if desired.
> Internal identifiers deliberately left as upstream (tracking fork): `Configuration.StatsApiUrl` /
> `KavitaPlusApiUrl` (now dead code), `DefaultOidcClientId = "kavita"`, and the `kavita.db` / `kavita.log` filenames.

---

## Phase 2 · Commit 5 — Frontend brand strings

- `index.html` `<title>` → Fathom; `site.webmanifest` `name`/`short_name` → Fathom.
- `_services/kavita-title.strategy.ts` — page-title fallback and ` (Kavita)` suffix → Fathom (class name/filename kept as upstream).
- `nav-header.component.html` navbar wordmark and `splash-container.component.html` login heading → Fathom.

## Phase 2 · Commit 6 — i18n source strings (`en.json`)

- Replaced **99** standalone `Kavita` → `Fathom` in the English source strings; **preserved 46** `Kavita+`
  labels (the dormant feature, per veneer-only). JSON validated.
- The other ~30 locale files under `assets/langs/` were **left untouched** to keep the diff merge-friendly;
  they'll show `Kavita` until re-translated (English is the default and primary audience language). Say the
  word and I'll run the same safe transform across all locales.

---

## Phase 2 · Commit 7 — Docs & Docker / CI

- `README.md` — full rewrite: Fathom branding + tagline, feature list (no commercial copy), Docker setup
  pointing at `fathomreader/fathom`, an **Attribution / NOTICE** section (fork of Kavita by majora2007,
  GPL-3.0, summary of changes, "does not claim original authorship"), and a License section crediting both
  projects. Removed all Donate / Kavita+ / Backers / Sponsors / PikaPods / OpenCollective / stats / Discord /
  Weblate content.
- `INSTALL.txt`, `SECURITY.md` — rebranded; security reports go to this repo's GitHub Security Advisories.
- `CONTRIBUTING.md` — added a fork note and repointed repo URLs to the fork (kept the real `Kavita.*` project
  names and dev commands, which are unchanged in a tracking fork).
- Docker image → `fathomreader/fathom` in `docker-build.sh` and the three release workflows. The workflows'
  `repository_owner == 'Kareadita'` push-gate was left in place, so CI publishing stays disabled on the fork
  until you wire up your own registry + secrets.

---

## Phase 2 · Commit 8 — Fix leftovers (all locales + doc links)

- **All locales rebranded** — ran the safe `Kavita` → `Fathom` transform (preserving `Kavita+`) across the
  28 non-English locale files: **1,640 strings**. JSON validated. Combined with `en.json`, every locale now reads Fathom.
- **Doc/Help links repointed** — `wiki.kavitareader.com` → `wiki.fathomreader.com` in `_models/wiki.ts`
  (26 links) and **48** doc URLs across 26 locale strings. UI source now has **0** `kavitareader.com` references.

> ⚠️ The Help/Wiki links now point at `wiki.fathomreader.com`, which has no docs yet — they'll 404 until you
> publish docs there or set up a redirect to the upstream Kavita wiki. Revert this commit if you'd rather keep
> functional upstream doc links for now. (Resolves residuals #3 and #4 below.)
> Still intentionally unchanged: dormant `Kavita+` labels (veneer-only), `CoverDbService`'s cover-art CDN
> (`www.kavitareader.com/CoversDB`, a feature, backend), and 3 code comments citing upstream issue URLs.

---

## Verification
- **Angular UI builds clean**: `npm install --legacy-peer-deps && npm run build` → exit 0 (only pre-existing
  Bootstrap SCSS deprecation warnings). The built `dist/browser/index.html` shows `<title>Fathom</title>`,
  navy `theme-color` `#0B1F3A`, and the Fathom manifest + regenerated favicon/logo.
- **Backend (C#) NOT compiled** — no .NET SDK in this environment. All backend edits are string/guard-only and
  syntactically safe, but run `dotnet build Kavita.sln` (or `./build.sh`) on a machine with the .NET 10 SDK to
  confirm before release.

## Known residuals (deliberate tracking-fork choices)
1. **Kavita+ is present but dormant** (veneer-only): scrobbling/external-metadata services, ~37 `kavita-plus`
   UI components, DB tables, and `Kavita+` labels remain. The buy/promo/donation funnel is neutralized and
   `plus.kavitareader.com` is never called. Full removal = optional Phase 3.
2. **Internal identifiers kept as upstream:** `Kavita.*` namespaces/projects/assemblies, the `Kavita` server
   binary + `/kavita` container paths, `kavita.db` / `kavita.log`, npm package `kavita-webui`, and the OIDC
   default client id `kavita`. Invisible to end users; renaming them would break upstream merges.
3. **Wiki "Help" links** (`_models/wiki.ts` ~25, plus a few in `en.json`) still point at `wiki.kavitareader.com`
   — they document the same features and there's no replacement yet. Repoint when you have your own docs.
4. **~30 non-English locale files** still say `Kavita` (English source is done; run the transform across all
   locales on request).
5. **`CoverDbService`** still fetches person/publisher cover art from `www.kavitareader.com/CoversDB` (a feature,
   not telemetry) — left functional; disable or self-host later if you prefer.
6. **3 code comments** cite upstream issue URLs (`github.com/Kareadita/Kavita/issues/...`) — kept as honest provenance.
7. **Unreferenced art** now safe to swap/remove: `Logo/{jetbrains,resharper,rider,dottrace,sentry}.svg`,
   `Logo/hosting-sponsor.png`, `UI/Web/src/assets/images/{logo.ai,kavita-book-cropped.png}`.

---

## Phase 3 · Commit 1 — Offline-first PWA (service worker)

Added an Angular service worker so the app loads and reads **offline** (built for at-sea / no-connectivity use). Upstream shipped **no** service worker at all.

- Added `@angular/service-worker` (^21.2.8) and `ngsw-config.json`.
- `angular.json` build target: `"serviceWorker": "ngsw-config.json"`.
- `main.ts`: `provideServiceWorker('ngsw-worker.js', { enabled: environment.production, registrationStrategy: 'registerWhenStable:30000' })` — **active in production builds only**; dev builds emit but don't register it.
- Caching: app shell prefetched; `/assets/**` (langs, fonts, icons, pdf-viewer) lazy-cached; cover & reader **images** cache-first (`performance`, 60d); other `GET /api/**` network-first with offline fallback (`freshness`, 14d).
- Verified: `npm run build` emits `dist/browser/ngsw-worker.js` + `ngsw.json`.

> This caches the app shell plus whatever you viewed while online. True "download-for-offline" (pre-fetching unread content for a voyage) is a larger follow-up.

---

## Phase 3 · Commit 2 — Polish residuals + Feature 2 plan

**Polish:**
- Removed 8 unreferenced upstream assets (0 code references): `Logo/{hosting-sponsor.png,jetbrains.svg,resharper.svg,rider.svg,dottrace.svg,sentry.svg}` and `UI/Web/src/assets/images/{logo.ai,kavita-book-cropped.png}`. `Logo/` now holds only `fathom.svg`.
- Rebranded the GitHub issue/discussion templates (`bug_report.yml`, `ideas.yml`, `config.yml`) to Fathom and repointed the discussions link to the fork. Workflows intentionally keep their real `Kavita.*` project paths and `Kareadita` owner-gates (which disable publishing on the fork).

**Feature 2 — maritime "Regulations" library type + IMO document number: DEFERRED (not implemented).**
Needs the .NET SDK (blocked here: `dot.net` 403) to add a `LibraryType`, a metadata column, and a
generated+tested EF migration; a blind hand-written migration would risk a broken DB. The full,
verified-path implementation plan is in `docs/phase3-maritime-library.md`.
