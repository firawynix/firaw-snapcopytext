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

    [Fact]
    public void ResolveStartupExecutable_PrefersLauncherBesideApplication()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"firaw-launcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string application = Path.Combine(directory, "Firaw.SnapCopyText.exe");
        string launcher = Path.Combine(directory, "Firaw.SnapCopyText.Launcher.exe");
        try
        {
            File.WriteAllText(launcher, string.Empty);

            Assert.Equal(launcher, StartupService.ResolveStartupExecutable(application));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
