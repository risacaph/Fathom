# [<img src="/Logo/fathom.svg" width="32" alt="">]() Fathom

**Your library. As deep as you need it.**

Fathom is a fast, self-hosted reading server for manga, webtoons, comics (cbz/cbr/zip/rar/7z/images)
and books (epub/pdf). Run your own server and share your reading collection with friends, family,
classmates, or crew — and keep it useful even when connectivity is patchy or absent.

[![License: GPL v3](https://img.shields.io/badge/license-GPLv3-blue.svg?style=flat)](LICENSE)

> **Fathom is a fork of [Kavita](https://github.com/Kareadita/Kavita).** See [Attribution / NOTICE](#attribution--notice) below.

## What Fathom provides
- Serve Manga/Webtoons/Comics (cbr, cbz, zip/rar/rar5, 7zip, raw images) and Books (epub, pdf)
- First-class responsive readers for phone, tablet, and desktop
- Customizable theming
- Rich metadata with filtering, searching, and smart filters
- Group reading material: Collections, Reading Lists (CBL import), Want to Read
- User management with role-based access, age restrictions, and OIDC
- OPDS support
- Localization support (English maintained; other languages inherited from upstream)

## Setup (Docker)
```bash
docker run -d \
  --name fathom \
  -p 5000:5000 \
  -v /path/to/config:/fathom/config \
  -v /path/to/library:/library \
  --restart unless-stopped \
  fathomreader/fathom:latest
```
Then open <http://localhost:5000> and set up your account and libraries.

> `fathomreader/fathom` is a placeholder image name — publish under your own Docker Hub / GHCR
> namespace and update this command accordingly.

## Build from source
See [CONTRIBUTING.md](CONTRIBUTING.md). In short: build the Angular UI under `UI/Web`, then build and
run `Fathom.Server` with the .NET SDK.

## Attribution / NOTICE
Fathom is a **fork of [Kavita](https://github.com/Kareadita/Kavita)**, created and maintained by
**[majora2007](https://github.com/majora2007)** and the Kavita contributors, and licensed under the
**GNU General Public License v3.0**.

Fathom is **not** affiliated with or endorsed by the Kavita project. The "Kavita" name and logo are
trademarks of their owner, are **not** covered by the GPL, and have been replaced in this fork.
"Kavita+" is a separate paid subscription operated by the original author — Fathom does not sell,
resell, or operate it, and its commercial/donation surfaces have been removed from this fork.

The underlying software is substantially the work of the Kavita project; **this fork does not claim
original authorship of it.**

### Summary of changes in this fork
- Rebranded to **Fathom** — name, logo, favicons/PWA icons, theme palette, and UI/API/OPDS/email strings.
- Removed the original author's commercial & donation surfaces (Kavita+ purchase funnel, Stripe promo
  codes, PayPal / OpenCollective / sponsor / backer links).
- Disabled anonymous usage telemetry — no data is sent to any stats server.
- Repointed brand domains to `fathomreader.com` and the container image to `fathomreader/fathom`.

A running, detailed log lives in [REBRAND-CHANGELOG.md](REBRAND-CHANGELOG.md).

## License
- [GNU GPL v3](LICENSE) — this fork remains free and open-source under the same license.
- Copyright © 2020–2026 Kavita contributors (majora2007) — original work.
- Copyright © 2026 Fathom — modifications.

This program is free software: you can redistribute it and/or modify it under the terms of the GNU
General Public License as published by the Free Software Foundation, either version 3 of the License,
or (at your option) any later version. It is distributed WITHOUT ANY WARRANTY. See [LICENSE](LICENSE).
