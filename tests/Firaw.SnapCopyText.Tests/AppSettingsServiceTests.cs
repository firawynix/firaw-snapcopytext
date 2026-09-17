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
                UseAltPrintScreen = false,
                UseControlPrintScreen = true,
                PrintScreenShortcut = "Shift + Print Screen",
                AltPrintScreenShortcut = "Ctrl + Alt + Print Screen",
                ControlPrintScreenShortcut = "Ctrl + Shift + Print Screen",
                PrintScreenMode = CaptureMode.Window,
                AltPrintScreenMode = CaptureMode.Region,
                ControlPrintScreenMode = CaptureMode.Monitor,
                StartWithWindows = true
            });

            CapturePreferences loaded = service.Load();

            Assert.Equal(CaptureMode.Monitor, loaded.DefaultMode);
            Assert.Equal("Ctrl + Alt + F9", loaded.Shortcut);
            Assert.False(loaded.UsePrintScreen);
            Assert.False(loaded.UseAltPrintScreen);
            Assert.True(loaded.UseControlPrintScreen);
            Assert.Equal("Shift + Print Screen", loaded.PrintScreenShortcut);
            Assert.Equal("Ctrl + Alt + Print Screen", loaded.AltPrintScreenShortcut);
            Assert.Equal("Ctrl + Shift + Print Screen", loaded.ControlPrintScreenShortcut);
            Assert.Equal(CaptureMode.Window, loaded.PrintScreenMode);
            Assert.Equal(CaptureMode.Region, loaded.AltPrintScreenMode);
            Assert.Equal(CaptureMode.Monitor, loaded.ControlPrintScreenMode);
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
            Assert.True(loaded.UseAltPrintScreen);
            Assert.True(loaded.UseControlPrintScreen);
            Assert.Equal("Print Screen", loaded.PrintScreenShortcut);
            Assert.Equal("Alt + Print Screen", loaded.AltPrintScreenShortcut);
            Assert.Equal("Ctrl + Print Screen", loaded.ControlPrintScreenShortcut);
            Assert.Equal(CaptureMode.Region, loaded.PrintScreenMode);
            Assert.Equal(CaptureMode.Monitor, loaded.AltPrintScreenMode);
            Assert.Equal(CaptureMode.Window, loaded.ControlPrintScreenMode);
            Assert.False(loaded.StartWithWindows);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_EnablesNewPrintScreenProfileForLegacySettings()
    {
        string path = Path.Combine(Path.GetTempPath(), $"firaw-settings-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                {
                  "DefaultMode": 0,
                  "Shortcut": "Ctrl + Shift + S",
                  "UsePrintScreen": true,
                  "StartWithWindows": true
                }
                """);

            CapturePreferences loaded = new AppSettingsService(path).Load();

            Assert.True(loaded.UsePrintScreen);
            Assert.True(loaded.UseAltPrintScreen);
            Assert.True(loaded.UseControlPrintScreen);
            Assert.Equal("Print Screen", loaded.PrintScreenShortcut);
            Assert.Equal("Alt + Print Screen", loaded.AltPrintScreenShortcut);
            Assert.Equal("Ctrl + Print Screen", loaded.ControlPrintScreenShortcut);
            Assert.Equal(CaptureMode.Region, loaded.PrintScreenMode);
            Assert.Equal(CaptureMode.Monitor, loaded.AltPrintScreenMode);
            Assert.Equal(CaptureMode.Window, loaded.ControlPrintScreenMode);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
