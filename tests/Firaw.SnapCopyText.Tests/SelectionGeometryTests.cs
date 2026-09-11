using System.Windows;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public class SelectionGeometryTests
{
    private static readonly Rect Bounds = new(0, 0, 800, 600);

    [Fact]
    public void Move_PreservesSizeAndClampsToBounds()
    {
        Rect result = SelectionGeometry.Move(new Rect(700, 500, 80, 70), 100, 100, Bounds);

        Assert.Equal(new Rect(720, 530, 80, 70), result);
    }

    [Fact]
    public void Resize_TopLeftChangesOnlyRequestedEdges()
    {
        Rect result = SelectionGeometry.Resize(
            new Rect(100, 100, 300, 200), ResizeHandle.TopLeft, 40, 25, Bounds);

        Assert.Equal(new Rect(140, 125, 260, 175), result);
    }

    [Fact]
    public void Resize_EnforcesMinimumSize()
    {
        Rect result = SelectionGeometry.Resize(
            new Rect(100, 100, 100, 100), ResizeHandle.Left, 200, 0, Bounds);

        Assert.Equal(16, result.Width);
        Assert.Equal(184, result.Left);
    }

    [Fact]
    public void Resize_ClampsToDesktopBounds()
    {
        Rect result = SelectionGeometry.Resize(
            new Rect(100, 100, 300, 200), ResizeHandle.BottomRight, 600, 500, Bounds);

        Assert.Equal(new Rect(100, 100, 700, 500), result);
    }
}
