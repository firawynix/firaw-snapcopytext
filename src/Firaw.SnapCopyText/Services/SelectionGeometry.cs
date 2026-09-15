using System.Windows;
using Firaw.SnapCopyText.Models;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Firaw.SnapCopyText.Services;

public static class SelectionGeometry
{
    public static Rect PlaceActionBar(
        Rect selection,
        Size actionBarSize,
        Rect bounds,
        double inset = 10)
    {
        double width = Math.Min(actionBarSize.Width, bounds.Width);
        double height = Math.Min(actionBarSize.Height, bounds.Height);
        double horizontalInset = Math.Min(inset, Math.Max(0, (selection.Width - width) / 2));
        double verticalInset = Math.Min(inset, Math.Max(0, (selection.Height - height) / 2));
        double insideLeft = Math.Clamp(selection.Right - width - horizontalInset, bounds.Left, bounds.Right - width);
        double insideTop = Math.Clamp(selection.Bottom - height - verticalInset, bounds.Top, bounds.Bottom - height);
        return new Rect(insideLeft, insideTop, width, height);
    }

    public static Rect Move(Rect selection, double deltaX, double deltaY, Rect bounds)
    {
        double left = Math.Clamp(selection.Left + deltaX, bounds.Left, bounds.Right - selection.Width);
        double top = Math.Clamp(selection.Top + deltaY, bounds.Top, bounds.Bottom - selection.Height);
        return new Rect(left, top, selection.Width, selection.Height);
    }

    public static Rect Resize(
        Rect selection,
        ResizeHandle handle,
        double deltaX,
        double deltaY,
        Rect bounds,
        double minimumSize = 16)
    {
        double left = selection.Left;
        double top = selection.Top;
        double right = selection.Right;
        double bottom = selection.Bottom;

        if (handle is ResizeHandle.TopLeft or ResizeHandle.Left or ResizeHandle.BottomLeft)
        {
            left = Math.Clamp(left + deltaX, bounds.Left, right - minimumSize);
        }

        if (handle is ResizeHandle.TopRight or ResizeHandle.Right or ResizeHandle.BottomRight)
        {
            right = Math.Clamp(right + deltaX, left + minimumSize, bounds.Right);
        }

        if (handle is ResizeHandle.TopLeft or ResizeHandle.Top or ResizeHandle.TopRight)
        {
            top = Math.Clamp(top + deltaY, bounds.Top, bottom - minimumSize);
        }

        if (handle is ResizeHandle.BottomLeft or ResizeHandle.Bottom or ResizeHandle.BottomRight)
        {
            bottom = Math.Clamp(bottom + deltaY, top + minimumSize, bounds.Bottom);
        }

        return new Rect(new Point(left, top), new Point(right, bottom));
    }
}
