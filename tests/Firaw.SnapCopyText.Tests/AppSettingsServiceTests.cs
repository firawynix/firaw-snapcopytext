using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class AppSettingsServiceTests
{
    [Fact]
    public void SaveAndLoad_PreservesCapturePreferences()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"firaw-settings-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "settings.json");

        try
        {
            var service = new AppSettingsService(path);
            service.Save(new CapturePreferences
            {
                DefaultMode = CaptureMode.Monitor,
                Shortcut = "Ctrl + Alt + F9",
                UsePrintScreen = false,
                StartWithWindows = true
            });

            CapturePreferences loaded = service.Load();

            Assert.Equal(CaptureMode.Monitor, loaded.DefaultMode);
            Assert.Equal("Ctrl + Alt + F9", loaded.Shortcut);
            Assert.False(loaded.UsePrintScreen);
            Assert.True(loaded.StartWithWindows);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void Load_ReturnsDefaultsForInvalidJson()
    {
        string path = Path.Combine(Path.GetTempPath(), $"firaw-settings-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, "not-json");
            CapturePreferences loaded = new AppSettingsService(path).Load();

            Assert.Equal(CaptureMode.Region, loaded.DefaultMode);
            Assert.Equal("Ctrl + Shift + S", loaded.Shortcut);
            Assert.True(loaded.UsePrintScreen);
            Assert.False(loaded.StartWithWindows);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
