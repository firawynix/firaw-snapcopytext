using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Services;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;

public sealed class HotkeyService : IDisposable
{
    private const int PrintScreenId = 0x5343;
    private const int FallbackId = 0x5344;
    private const int AltPrintScreenId = 0x5346;
    private const int ControlPrintScreenId = 0x5347;
    private const int WmHotkey = 0x0312;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const int WhKeyboardLl = 13;
    private const int VkSnapshot = 0x2C;
    private const int VkControl = 0x11;
    private const int VkShift = 0x10;
    private const int VkMenu = 0x12;
    private const int VkLeftWindows = 0x5B;
    private const int VkRightWindows = 0x5C;
    private const uint LlkhfAltDown = 0x20;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;

    private readonly Window _window;
    private readonly LowLevelKeyboardProcedure _keyboardProcedure;
    private CapturePreferences _settings = new();
    private nint _windowHandle;
    private nint _keyboardHook;
    private HwndSource? _source;
    private bool _suppressPrintScreenUntilKeyUp;
    private bool _disposed;

    public bool PrintScreenShortcutActive { get; private set; }
    public bool AltPrintScreenShortcutActive { get; private set; }
    public bool ControlPrintScreenShortcutActive { get; private set; }
    public bool FallbackRegistered { get; private set; }
    public bool UsesKeyboardHook => _keyboardHook != nint.Zero;
    public string FallbackLabel { get; private set; } = "Ctrl + Shift + S";
    public event Action<CaptureMode>? CaptureRequested;

    public HotkeyService(Window window)
    {
        _window = window;
        _keyboardProcedure = KeyboardProcedure;
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
        _settings = settings.Clone();

        if (!TryParseShortcut(settings.Shortcut, out uint modifiers, out Key key, out string label))
        {
            TryParseShortcut("Ctrl + Shift + S", out modifiers, out key, out label);
        }
        FallbackLabel = label;

        TryParsePrintScreenShortcut(settings.PrintScreenShortcut, out uint printModifiers, out _, out _);
        TryParsePrintScreenShortcut(settings.AltPrintScreenShortcut, out uint altPrintModifiers, out _, out _);
        TryParsePrintScreenShortcut(settings.ControlPrintScreenShortcut, out uint controlPrintModifiers, out _, out _);

        bool anyPresetEnabled = settings.UsePrintScreen ||
                                settings.UseAltPrintScreen ||
                                settings.UseControlPrintScreen;
        if (anyPresetEnabled)
        {
            _keyboardHook = SetWindowsHookEx(
                WhKeyboardLl,
                _keyboardProcedure,
                GetModuleHandle(null),
                0);
        }

        if (_keyboardHook != nint.Zero)
        {
            PrintScreenShortcutActive = settings.UsePrintScreen;
            AltPrintScreenShortcutActive = settings.UseAltPrintScreen;
            ControlPrintScreenShortcutActive = settings.UseControlPrintScreen;
        }
        else
        {
            PrintScreenShortcutActive = RegisterPreset(settings.UsePrintScreen, PrintScreenId, printModifiers);
            AltPrintScreenShortcutActive = RegisterPreset(settings.UseAltPrintScreen, AltPrintScreenId, altPrintModifiers);
            ControlPrintScreenShortcutActive = RegisterPreset(settings.UseControlPrintScreen, ControlPrintScreenId, controlPrintModifiers);
        }

        bool customShortcutIsManagedPrintScreen = key == Key.PrintScreen &&
                                                  ((settings.UsePrintScreen && modifiers == printModifiers) ||
                                                   (settings.UseAltPrintScreen && modifiers == altPrintModifiers) ||
                                                   (settings.UseControlPrintScreen && modifiers == controlPrintModifiers));
        if (!customShortcutIsManagedPrintScreen)
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

    private bool RegisterPreset(bool enabled, int id, uint modifiers) =>
        enabled && RegisterHotKey(_windowHandle, id, modifiers | ModNoRepeat, VkSnapshot);

    private void UnregisterCurrentHotkeys()
    {
        if (_keyboardHook != nint.Zero)
        {
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = nint.Zero;
        }

        if (_windowHandle != nint.Zero)
        {
            UnregisterHotKey(_windowHandle, PrintScreenId);
            UnregisterHotKey(_windowHandle, AltPrintScreenId);
            UnregisterHotKey(_windowHandle, ControlPrintScreenId);
            UnregisterHotKey(_windowHandle, FallbackId);
        }

        _suppressPrintScreenUntilKeyUp = false;
        PrintScreenShortcutActive = false;
        AltPrintScreenShortcutActive = false;
        ControlPrintScreenShortcutActive = false;
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

    public static bool TryParsePrintScreenShortcut(
        string? shortcut,
        out uint nativeModifiers,
        out ModifierKeys modifiers,
        out string normalizedLabel)
    {
        modifiers = ModifierKeys.None;
        if (!TryParseShortcut(shortcut, out nativeModifiers, out Key key, out normalizedLabel) ||
            key != Key.PrintScreen)
        {
            nativeModifiers = 0;
            normalizedLabel = string.Empty;
            return false;
        }

        if ((nativeModifiers & ModControl) != 0) modifiers |= ModifierKeys.Control;
        if ((nativeModifiers & ModShift) != 0) modifiers |= ModifierKeys.Shift;
        if ((nativeModifiers & ModAlt) != 0) modifiers |= ModifierKeys.Alt;
        return true;
    }

    public static bool TryGetPresetMode(
        CapturePreferences settings,
        ModifierKeys modifiers,
        out CaptureMode mode)
    {
        ModifierKeys relevant = modifiers &
                                (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt | ModifierKeys.Windows);
        if (settings.UsePrintScreen && ShortcutMatches(settings.PrintScreenShortcut, relevant))
        {
            mode = settings.PrintScreenMode;
            return true;
        }
        if (settings.UseAltPrintScreen && ShortcutMatches(settings.AltPrintScreenShortcut, relevant))
        {
            mode = settings.AltPrintScreenMode;
            return true;
        }
        if (settings.UseControlPrintScreen && ShortcutMatches(settings.ControlPrintScreenShortcut, relevant))
        {
            mode = settings.ControlPrintScreenMode;
            return true;
        }

        mode = default;
        return false;
    }

    private static bool ShortcutMatches(string shortcut, ModifierKeys modifiers) =>
        TryParsePrintScreenShortcut(shortcut, out _, out ModifierKeys expected, out _) &&
        expected == modifiers;

    private nint KeyboardProcedure(int code, nint message, nint dataPointer)
    {
        if (code >= 0)
        {
            int keyboardMessage = message.ToInt32();
            KeyboardHookData data = Marshal.PtrToStructure<KeyboardHookData>(dataPointer);
            if (data.VirtualKeyCode == VkSnapshot)
            {
                if (keyboardMessage is WmKeyDown or WmSysKeyDown)
                {
                    if (_suppressPrintScreenUntilKeyUp)
                    {
                        return 1;
                    }

                    ModifierKeys modifiers = ReadModifiers(data.Flags);
                    if (TryGetPresetMode(_settings, modifiers, out CaptureMode mode))
                    {
                        _suppressPrintScreenUntilKeyUp = true;
                        _window.Dispatcher.BeginInvoke(() => CaptureRequested?.Invoke(mode));
                        return 1;
                    }
                }
                else if (keyboardMessage is WmKeyUp or WmSysKeyUp && _suppressPrintScreenUntilKeyUp)
                {
                    _suppressPrintScreenUntilKeyUp = false;
                    return 1;
                }
            }
        }

        return CallNextHookEx(_keyboardHook, code, message, dataPointer);
    }

    private static ModifierKeys ReadModifiers(uint flags)
    {
        ModifierKeys modifiers = ModifierKeys.None;
        if ((flags & LlkhfAltDown) != 0 || IsKeyDown(VkMenu)) modifiers |= ModifierKeys.Alt;
        if (IsKeyDown(VkControl)) modifiers |= ModifierKeys.Control;
        if (IsKeyDown(VkShift)) modifiers |= ModifierKeys.Shift;
        if (IsKeyDown(VkLeftWindows) || IsKeyDown(VkRightWindows)) modifiers |= ModifierKeys.Windows;
        return modifiers;
    }

    private static bool IsKeyDown(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    private nint WindowProcedure(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message != WmHotkey)
        {
            return nint.Zero;
        }

        CaptureMode? mode = wParam.ToInt32() switch
        {
            PrintScreenId => _settings.PrintScreenMode,
            AltPrintScreenId => _settings.AltPrintScreenMode,
            ControlPrintScreenId => _settings.ControlPrintScreenMode,
            FallbackId => _settings.DefaultMode,
            _ => null
        };
        if (mode.HasValue)
        {
            handled = true;
            CaptureRequested?.Invoke(mode.Value);
        }

        return nint.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct KeyboardHookData
    {
        public readonly uint VirtualKeyCode;
        public readonly uint ScanCode;
        public readonly uint Flags;
        public readonly uint Time;
        public readonly nuint ExtraInfo;
    }

    private delegate nint LowLevelKeyboardProcedure(int code, nint message, nint dataPointer);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(
        int hookType,
        LowLevelKeyboardProcedure callback,
        nint moduleHandle,
        uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hookHandle);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hookHandle, int code, nint message, nint dataPointer);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? moduleName);
}
