using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Services;

public sealed class HotkeyService : IDisposable
{
    private const int PrintScreenId = 0x5343;
    private const int FallbackId = 0x5344;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkSnapshot = 0x2C;

    private readonly Window _window;
    private nint _windowHandle;
    private HwndSource? _source;
    private bool _disposed;

    public bool PrintScreenRegistered { get; private set; }
    public bool FallbackRegistered { get; private set; }
    public string FallbackLabel { get; private set; } = "Ctrl + Shift + S";
    public event EventHandler? CaptureRequested;

    public HotkeyService(Window window)
    {
        _window = window;
    }

    public void Initialize(CapturePreferences settings)
    {
        if (_windowHandle != nint.Zero)
        {
            return;
        }

        _windowHandle = new WindowInteropHelper(_window).Handle;
        if (_windowHandle == nint.Zero)
        {
            throw new InvalidOperationException("A janela precisa estar inicializada antes de registrar atalhos.");
        }

        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(WindowProcedure);

        Apply(settings);
    }

    public void Apply(CapturePreferences settings)
    {
        if (_windowHandle == nint.Zero)
        {
            return;
        }

        UnregisterCurrentHotkeys();

        if (!TryParseShortcut(settings.Shortcut, out uint modifiers, out Key key, out string label))
        {
            TryParseShortcut("Ctrl + Shift + S", out modifiers, out key, out label);
        }
        FallbackLabel = label;
        bool customShortcutIsPrintScreen = key == Key.PrintScreen && modifiers == 0;

        if (settings.UsePrintScreen)
        {
            PrintScreenRegistered = RegisterHotKey(_windowHandle, PrintScreenId, ModNoRepeat, VkSnapshot);
        }

        if (!customShortcutIsPrintScreen || !settings.UsePrintScreen)
        {
            FallbackRegistered = RegisterHotKey(
                _windowHandle,
                FallbackId,
                modifiers | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(key));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnregisterCurrentHotkeys();

        _source?.RemoveHook(WindowProcedure);
    }

    public void Suspend() => UnregisterCurrentHotkeys();

    private void UnregisterCurrentHotkeys()
    {
        if (_windowHandle == nint.Zero)
        {
            return;
        }

        if (PrintScreenRegistered)
        {
            UnregisterHotKey(_windowHandle, PrintScreenId);
        }
        if (FallbackRegistered)
        {
            UnregisterHotKey(_windowHandle, FallbackId);
        }

        PrintScreenRegistered = false;
        FallbackRegistered = false;
    }

    public static bool TryCreateShortcutLabel(ModifierKeys modifiers, Key key, out string label)
    {
        label = string.Empty;
        if (key is Key.None or Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.Escape or Key.Tab or Key.Enter)
        {
            return false;
        }

        bool isFunctionKey = key >= Key.F1 && key <= Key.F24;
        bool isDedicatedShortcutKey = key == Key.PrintScreen;
        ModifierKeys allowed = modifiers & (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt);
        if (allowed == ModifierKeys.None && !isFunctionKey && !isDedicatedShortcutKey)
        {
            return false;
        }

        List<string> parts = [];
        if (allowed.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (allowed.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (allowed.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        parts.Add(key == Key.PrintScreen ? "Print Screen" : key.ToString().ToUpperInvariant());
        label = string.Join(" + ", parts);
        return true;
    }

    public static bool TryParseShortcut(
        string? shortcut,
        out uint modifiers,
        out Key key,
        out string normalizedLabel)
    {
        modifiers = 0;
        key = Key.None;
        normalizedLabel = string.Empty;
        if (string.IsNullOrWhiteSpace(shortcut))
        {
            return false;
        }

        ModifierKeys modifierKeys = ModifierKeys.None;
        foreach (string rawPart in shortcut.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (rawPart.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                rawPart.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifierKeys |= ModifierKeys.Control;
            }
            else if (rawPart.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifierKeys |= ModifierKeys.Shift;
            }
            else if (rawPart.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifierKeys |= ModifierKeys.Alt;
            }
            else if (rawPart.Equals("Print Screen", StringComparison.OrdinalIgnoreCase) ||
                     rawPart.Equals("PrtSc", StringComparison.OrdinalIgnoreCase))
            {
                key = Key.PrintScreen;
            }
            else if (!Enum.TryParse(rawPart, ignoreCase: true, out key))
            {
                return false;
            }
        }

        if (!TryCreateShortcutLabel(modifierKeys, key, out normalizedLabel))
        {
            return false;
        }

        if (modifierKeys.HasFlag(ModifierKeys.Control)) modifiers |= ModControl;
        if (modifierKeys.HasFlag(ModifierKeys.Shift)) modifiers |= ModShift;
        if (modifierKeys.HasFlag(ModifierKeys.Alt)) modifiers |= ModAlt;
        return true;
    }

    private nint WindowProcedure(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == WmHotkey && (wParam.ToInt32() == PrintScreenId || wParam.ToInt32() == FallbackId))
        {
            handled = true;
            CaptureRequested?.Invoke(this, EventArgs.Empty);
        }

        return nint.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);
}
