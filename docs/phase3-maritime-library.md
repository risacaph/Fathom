# Phase 3 (deferred): Maritime "Regulations" library type + IMO document number

**Status: not implemented.** It needs the **.NET SDK** to add a `LibraryType` value, add a metadata
column, and **generate + test an EF Core migration**. The rebrand environment blocks the SDK (`dot.net`
returns HTTP 403) and a blind, hand-written migration risks a broken database — so this is a ready-to-run
plan to execute in a .NET 10 environment (or I can do it in a .NET-capable session).

File paths below were verified against this repo.

## Goal
- A new library type **Regulations** (maritime regs — SOLAS / MARPOL / IMO circulars) that scans like a
  Book library (PDF/EPUB).
- A per-series metadata field **IMO Document No.** (e.g. `MSC.1/Circ.1234`), editable in the UI and shown
  on the series detail page.

## Backend (C#)
1. **Enum** — `Kavita.Models/Entities/Enums/LibraryType.cs`: add `Regulations = <next unused int>`.
   Do **not** reorder existing values (persisted as ints).
2. **Handle it** — `grep -rn "LibraryType\." --include=*.cs`. Treat `Regulations` like `LibraryType.Book`
   in parser/scanner selection, reader-type resolution, and any `LibraryTypeHelper`. Exhaustive `switch`
   statements will fail to compile until each handles the new case — that's your safety net.
3. **Metadata column** — `Kavita.Models/Entities/Metadata/SeriesMetadata.cs`, mirroring an existing
   string field + its lock flag:
   ```csharp
   public string ImoNumber { get; set; } = string.Empty;
   public bool ImoNumberLocked { get; set; } = false;
   ```
   Wire defaults in `Kavita.Models/Builders/SeriesMetadataBuilder.cs`.
4. **Migration** (needs the SDK — do not hand-write):
   ```
   dotnet ef migrations add AddImoNumberToSeriesMetadata \
     --project Kavita.Database/Kavita.Database.csproj \
     --startup-project Kavita.Server/Kavita.Server.csproj \
     --context Kavita.Database.DataContext -o Migrations
   ```
   Review the generated `AddColumn` and the auto-updated `DataContextModelSnapshot.cs`.
5. **DTOs** — add `ImoNumber` (+ `ImoNumberLocked`) to `Kavita.Models/DTOs/SeriesMetadataDto.cs` and
   `Kavita.Models/DTOs/UpdateSeriesMetadataDto.cs`.
6. **Mapping + persistence** — add the field to the AutoMapper profile
   (`grep -rn "SeriesMetadataDto" Kavita.Services`) and to `SeriesService.UpdateSeriesMetadata`, honoring
   the `*Locked` flag exactly like sibling fields.
7. *(optional)* **ComicInfo/OPF import** — map a custom/`<Number>` tag → `ImoNumber` in the metadata reader.

## Frontend (Angular)
1. **Enum** — `UI/Web/src/app/_models/library/library.ts`: add `Regulations` (same int as backend).
2. **Label + icon** — library-type name pipe + i18n; add a "Maritime Regulations" label and an icon.
3. **Model** — add `imoNumber` to the series-metadata interface(s) under `_models/metadata/`.
4. **Edit UI** — in the edit-series metadata modal add an **"IMO Document No."** input (+ lock toggle),
   shown when `library.type === LibraryType.Regulations`.
5. **Display** — show it on the series-detail metadata panel.
6. **i18n** — add label keys to `en.json`, then run the locale transform (`scripts`-style) for the rest.

## Verify
- `dotnet build Kavita.sln` (compile; catches missed switches).
- Start the server against a **test** DB so the migration applies; confirm the column exists and metadata
  round-trips through the API.
- `cd UI/Web && npm run build`.

## No-migration interim
Add only the `Regulations` `LibraryType` (enum = int, **no schema change**) and stash the IMO number in an
existing free-text field (relabeled "IMO Document No." in the UI for Regulations libraries). Add the
dedicated column later via the migration above.

## Effort / risk
| Part | Effort | Risk |
| --- | --- | --- |
| New `Regulations` LibraryType | S | Low (compiler-guarded) |
| `ImoNumber` column + EF migration | M | Low-moderate **with** SDK; **high** without |
| Frontend field + display | S–M | Low |
