namespace Firaw.SnapCopyText.Models;

public sealed class CapturePreferences
{
    public CaptureMode DefaultMode { get; set; } = CaptureMode.Region;
    public string Shortcut { get; set; } = "Ctrl + Shift + S";
    public bool UsePrintScreen { get; set; } = true;
    public bool UseAltPrintScreen { get; set; } = true;
    public bool UseControlPrintScreen { get; set; } = true;
    public string PrintScreenShortcut { get; set; } = "Print Screen";
    public string AltPrintScreenShortcut { get; set; } = "Alt + Print Screen";
    public string ControlPrintScreenShortcut { get; set; } = "Ctrl + Print Screen";
    public CaptureMode PrintScreenMode { get; set; } = CaptureMode.Region;
    public CaptureMode AltPrintScreenMode { get; set; } = CaptureMode.Monitor;
    public CaptureMode ControlPrintScreenMode { get; set; } = CaptureMode.Window;
    public bool StartWithWindows { get; set; }

    public CapturePreferences Clone() => new()
    {
        DefaultMode = DefaultMode,
        Shortcut = Shortcut,
        UsePrintScreen = UsePrintScreen,
        UseAltPrintScreen = UseAltPrintScreen,
        UseControlPrintScreen = UseControlPrintScreen,
        PrintScreenShortcut = PrintScreenShortcut,
        AltPrintScreenShortcut = AltPrintScreenShortcut,
        ControlPrintScreenShortcut = ControlPrintScreenShortcut,
        PrintScreenMode = PrintScreenMode,
        AltPrintScreenMode = AltPrintScreenMode,
        ControlPrintScreenMode = ControlPrintScreenMode,
        StartWithWindows = StartWithWindows
    };
}
