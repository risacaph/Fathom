using System.Runtime.InteropServices;

namespace Fathom.Viewer;

/// <summary>
/// Win32 interop for the anti-capture protection. <see cref="WDA_EXCLUDEFROMCAPTURE"/> makes the window's
/// pixels render as black in every screenshot / screen-recording / remote-desktop capture — the strongest
/// on-device deterrent Windows provides (used by protected-media apps).
/// </summary>
internal static class NativeMethods
{
    public const uint WDA_NONE = 0x0;
    public const uint WDA_MONITOR = 0x1;
    public const uint WDA_EXCLUDEFROMCAPTURE = 0x11; // Windows 10 version 2004+ (build 19041)

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowDisplayAffinity(nint hWnd, uint dwAffinity);

    /// <summary>
    /// Exclude the window from capture, falling back to WDA_MONITOR on pre-2004 builds.
    /// Returns true if any protection was applied.
    /// </summary>
    public static bool ProtectFromCapture(nint hWnd)
    {
        if (hWnd == 0) return false;
        return SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE)
               || SetWindowDisplayAffinity(hWnd, WDA_MONITOR);
    }
}
