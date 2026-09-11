using System.Windows;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class CaptureServiceTests
{
    [Fact]
    public void NormalizeSelection_SupportsDraggingUpAndLeft()
    {
        Int32Rect result = CaptureService.NormalizeSelection(90, 80, 10, 20, new Int32Rect(0, 0, 100, 100));

        Assert.Equal(new Int32Rect(10, 20, 80, 60), result);
    }

    [Fact]
    public void NormalizeSelection_ClampsToImageBounds()
    {
        Int32Rect result = CaptureService.NormalizeSelection(-20, -10, 150, 120, new Int32Rect(0, 0, 100, 80));

        Assert.Equal(new Int32Rect(0, 0, 100, 80), result);
    }

    [Fact]
    public void NormalizeSelection_ReturnsEmptyForOutsideSelection()
    {
        Int32Rect result = CaptureService.NormalizeSelection(120, 120, 140, 150, new Int32Rect(0, 0, 100, 100));

        Assert.Equal(Int32Rect.Empty, result);
    }
}
