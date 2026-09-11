using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class StartupServiceTests
{
    [Fact]
    public void QuoteExecutablePath_ProtectsPathsWithSpaces()
    {
        string command = StartupService.QuoteExecutablePath(@"C:\Program Files\Firaw\Firaw.exe");

        Assert.Equal("\"C:\\Program Files\\Firaw\\Firaw.exe\"", command);
    }
}
