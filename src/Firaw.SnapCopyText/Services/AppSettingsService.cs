using System.Text.Json;
using System.IO;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Services;

public sealed class AppSettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppSettingsService(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Firaw",
            "SnapCopyText",
            "settings.json");
    }

    public CapturePreferences Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new CapturePreferences();
            }

            CapturePreferences? settings = JsonSerializer.Deserialize<CapturePreferences>(
                File.ReadAllText(_settingsPath),
                _jsonOptions);
            return Sanitize(settings);
        }
        catch
        {
            return new CapturePreferences();
        }
    }

    public void Save(CapturePreferences settings)
    {
        CapturePreferences safeSettings = Sanitize(settings);
        string? directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(safeSettings, _jsonOptions));
    }

    private static CapturePreferences Sanitize(CapturePreferences? settings)
    {
        CaptureMode mode = settings?.DefaultMode ?? CaptureMode.Region;
        string shortcut = settings?.Shortcut ?? "Ctrl + Shift + S";
        return new CapturePreferences
        {
            DefaultMode = Enum.IsDefined(mode) ? mode : CaptureMode.Region,
            Shortcut = HotkeyService.TryParseShortcut(shortcut, out _, out _, out string normalized)
                ? normalized
                : "Ctrl + Shift + S",
            UsePrintScreen = settings?.UsePrintScreen ?? true,
            UseAltPrintScreen = settings?.UseAltPrintScreen ?? true,
            UseControlPrintScreen = settings?.UseControlPrintScreen ?? true,
            StartWithWindows = settings?.StartWithWindows ?? false
        };
    }
}
