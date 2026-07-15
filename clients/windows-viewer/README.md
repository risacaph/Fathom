# Fathom Secure Viewer (Windows)

A protected, watermarked document viewer for a Fathom server, distributed as a Windows **MSI**. It is the
client half of the DRM-lite viewer; the server half is the `api/protectedreader` endpoints in `Fathom.Server`.

## What it protects against

| Threat | Mitigation |
| --- | --- |
| Downloading the source file | The client only ever receives **rendered page images**, never the `.pdf`/`.epub`. Images are held in memory and never written to disk. |
| Screenshots / screen-recording | `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)` — the window renders **black** in any capture (Windows 10 2004+; falls back to `WDA_MONITOR`). |
| Copy / print / save | Those shortcuts are swallowed; there is no save/print/copy UI and no text to select (pages are images). |
| Camera photo of the screen (the "analog hole") | Cannot be prevented by any viewer. Each page is **watermarked server-side with the signed-in user + timestamp**, so a leaked photo is traceable to who leaked it. |

> Honest framing: this is a strong deterrent stack, not unbreakable DRM. Anything a screen can display can, in
> principle, be photographed. The design makes casual copying hard and deliberate leaks attributable.

## Build

The project targets `net10.0-windows` (WPF) and **only builds on Windows**.

```powershell
dotnet publish clients/windows-viewer/Fathom.Viewer/Fathom.Viewer.csproj -c Release -r win-x64 --self-contained true -o publish
dotnet tool install --global wix
wix build clients/windows-viewer/installer/Package.wxs -bindpath PublishDir=publish -o FathomSecureViewer.msi
```

Or trigger the **"Windows Secure Viewer (MSI)"** GitHub Actions workflow (manual dispatch), which publishes the
MSI as a draft release asset.

## Use

Double-click the MSI (per-user install, no admin). Launch **Fathom Secure Viewer**, enter your server URL and
sign in, search for a document, and open it. Navigate pages with Prev/Next.
