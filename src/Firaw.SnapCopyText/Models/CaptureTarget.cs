using System.Drawing;

namespace Firaw.SnapCopyText.Models;

public sealed record CaptureTarget(Rectangle Bounds, string Label, nint WindowHandle = default)
{
    public string SizeLabel => $"{Bounds.Width} × {Bounds.Height}";
}
