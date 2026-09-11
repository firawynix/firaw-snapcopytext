using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Firaw.SnapCopyText.Services;

public static class WindowBrandingService
{
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;
    private const int FirawCyanColorRef = 0x00E6D319;
    private const int FirawDarkColorRef = 0x00141007;

    public static void Apply(Window window)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        int captionColor = FirawCyanColorRef;
        int textColor = FirawDarkColorRef;
        _ = DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref captionColor, Marshal.SizeOf<int>());
        _ = DwmSetWindowAttribute(handle, DwmwaTextColor, ref textColor, Marshal.SizeOf<int>());
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        nint windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}
