using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class StartupServiceTests
{
    [Fact]
    public void BuildStartupCommand_QuotesPathAndStartsInBackground()
    {
        string command = StartupService.BuildStartupCommand(@"C:\Program Files\Firaw\Firaw.exe");

        Assert.Equal("\"C:\\Program Files\\Firaw\\Firaw.exe\" --background", command);
    }
}
