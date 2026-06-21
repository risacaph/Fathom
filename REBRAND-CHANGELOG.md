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
