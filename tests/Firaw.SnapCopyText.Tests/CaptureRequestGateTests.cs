using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class CaptureRequestGateTests
{
    [Fact]
    public void TryEnter_BlocksDuplicateUntilExit()
    {
        var gate = new CaptureRequestGate();

        Assert.True(gate.TryEnter());
        Assert.False(gate.TryEnter());

        gate.Exit();
        Assert.True(gate.TryEnter());
    }
}
