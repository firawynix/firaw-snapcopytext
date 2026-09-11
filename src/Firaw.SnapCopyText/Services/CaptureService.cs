using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Firaw.SnapCopyText.Services;

public sealed record DesktopSnapshot(BitmapSource Image, Rectangle ScreenBounds);

public sealed class CaptureService
{
    public DesktopSnapshot CaptureVirtualScreen()
    {
        Rectangle bounds = System.Windows.Forms.SystemInformation.VirtualScreen;

        using var bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        }

        nint handle = bitmap.GetHbitmap();
        try
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                nint.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return new DesktopSnapshot(source, bounds);
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    public BitmapSource Crop(BitmapSource source, Int32Rect region)
    {
        Int32Rect imageBounds = new(0, 0, source.PixelWidth, source.PixelHeight);
        Int32Rect safeRegion = Intersect(region, imageBounds);
        if (safeRegion.Width < 2 || safeRegion.Height < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(region), "A seleção precisa ter pelo menos 2 × 2 pixels.");
        }

        var cropped = new CroppedBitmap(source, safeRegion);
        cropped.Freeze();
        return cropped;
    }

    public static Int32Rect NormalizeSelection(
        double startX,
        double startY,
        double endX,
        double endY,
        Int32Rect limits)
    {
        int left = (int)Math.Floor(Math.Min(startX, endX));
        int top = (int)Math.Floor(Math.Min(startY, endY));
        int right = (int)Math.Ceiling(Math.Max(startX, endX));
        int bottom = (int)Math.Ceiling(Math.Max(startY, endY));

        return Intersect(new Int32Rect(left, top, right - left, bottom - top), limits);
    }

    private static Int32Rect Intersect(Int32Rect first, Int32Rect second)
    {
        int left = Math.Max(first.X, second.X);
        int top = Math.Max(first.Y, second.Y);
        int right = Math.Min(first.X + first.Width, second.X + second.Width);
        int bottom = Math.Min(first.Y + first.Height, second.Y + second.Height);

        return right <= left || bottom <= top
            ? Int32Rect.Empty
            : new Int32Rect(left, top, right - left, bottom - top);
    }

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint objectHandle);
}
