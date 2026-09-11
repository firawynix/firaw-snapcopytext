using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Services;

public sealed class WindowSelectionService
{
    private const int DwmwaExtendedFrameBounds = 9;
    private const int DwmwaCloaked = 14;

    public IReadOnlyList<CaptureTarget> GetVisibleWindows()
    {
        List<CaptureTarget> targets = [];
        uint currentProcessId = (uint)Environment.ProcessId;

        EnumWindows((windowHandle, _) =>
        {
            if (!IsWindowVisible(windowHandle) || IsIconic(windowHandle))
            {
                return true;
            }

            GetWindowThreadProcessId(windowHandle, out uint processId);
            if (processId == currentProcessId || IsCloaked(windowHandle))
            {
                return true;
            }

            int titleLength = GetWindowTextLength(windowHandle);
            if (titleLength <= 0)
            {
                return true;
            }

            var title = new StringBuilder(titleLength + 1);
            _ = GetWindowText(windowHandle, title, title.Capacity);
            if (!TryGetBounds(windowHandle, out Rectangle bounds) || bounds.Width < 40 || bounds.Height < 40)
            {
                return true;
            }

            targets.Add(new CaptureTarget(bounds, title.ToString(), windowHandle));
            return true;
        }, nint.Zero);

        return targets;
    }

    public IReadOnlyList<CaptureTarget> GetMonitors() => System.Windows.Forms.Screen.AllScreens
        .Select((screen, index) => new CaptureTarget(
            screen.Bounds,
            $"Monitor {index + 1}{(screen.Primary ? " • Principal" : string.Empty)}"))
        .ToArray();

    private static bool TryGetBounds(nint windowHandle, out Rectangle bounds)
    {
        if (DwmGetWindowAttribute(
                windowHandle,
                DwmwaExtendedFrameBounds,
                out NativeRect frame,
                Marshal.SizeOf<NativeRect>()) != 0 &&
            !GetWindowRect(windowHandle, out frame))
        {
            bounds = Rectangle.Empty;
            return false;
        }

        bounds = Rectangle.FromLTRB(frame.Left, frame.Top, frame.Right, frame.Bottom);
        return true;
    }

    private static bool IsCloaked(nint windowHandle)
    {
        int cloaked = 0;
        return DwmGetWindowAttribute(
                   windowHandle,
                   DwmwaCloaked,
                   out cloaked,
                   Marshal.SizeOf<int>()) == 0 && cloaked != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private delegate bool EnumWindowsCallback(nint windowHandle, nint parameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsCallback callback, nint parameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint windowHandle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(nint windowHandle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint windowHandle, StringBuilder text, int maximumCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint windowHandle, out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(nint windowHandle, out NativeRect rectangle);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        nint windowHandle,
        int attribute,
        out NativeRect value,
        int valueSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        nint windowHandle,
        int attribute,
        out int value,
        int valueSize);
}
