namespace Firaw.SnapCopyText.Models;

public sealed class CapturePreferences
{
    public CaptureMode DefaultMode { get; set; } = CaptureMode.Region;
    public string Shortcut { get; set; } = "Ctrl + Shift + S";
    public bool UsePrintScreen { get; set; } = true;
    public bool StartWithWindows { get; set; }

    public CapturePreferences Clone() => new()
    {
        DefaultMode = DefaultMode,
        Shortcut = Shortcut,
        UsePrintScreen = UsePrintScreen,
        StartWithWindows = StartWithWindows
    };
}
