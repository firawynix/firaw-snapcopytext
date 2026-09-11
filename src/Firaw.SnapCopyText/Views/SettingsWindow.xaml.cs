using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;

namespace Firaw.SnapCopyText.Views;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

public partial class SettingsWindow : Window
{
    private const int RecorderPrintScreenId = 0x5345;
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkSnapshot = 0x2C;

    private string _shortcut;
    private nint _windowHandle;
    private HwndSource? _windowSource;
    private bool _printScreenRegistered;
    public CapturePreferences? SavedPreferences { get; private set; }

    public SettingsWindow(CapturePreferences preferences)
    {
        InitializeComponent();

        DefaultModeCombo.ItemsSource = new[]
        {
            new ModeOption(CaptureMode.Region, "Selecionar região"),
            new ModeOption(CaptureMode.Window, "Escolher janela"),
            new ModeOption(CaptureMode.Monitor, "Escolher monitor")
        };

        DefaultModeCombo.SelectedValue = preferences.DefaultMode;
        _shortcut = preferences.Shortcut;
        ShortcutInput.Text = _shortcut;
        UsePrintScreenCheck.IsChecked = preferences.UsePrintScreen;
        StartWithWindowsCheck.IsChecked = preferences.StartWithWindows;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _windowHandle = new WindowInteropHelper(this).Handle;
        _windowSource = HwndSource.FromHwnd(_windowHandle);
        _windowSource?.AddHook(WindowProcedure);
        _printScreenRegistered = RegisterHotKey(
            _windowHandle,
            RecorderPrintScreenId,
            ModNoRepeat,
            VkSnapshot);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_printScreenRegistered)
        {
            UnregisterHotKey(_windowHandle, RecorderPrintScreenId);
        }
        _windowSource?.RemoveHook(WindowProcedure);
        base.OnClosed(e);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SavedPreferences = new CapturePreferences
        {
            DefaultMode = DefaultModeCombo.SelectedValue is CaptureMode mode ? mode : CaptureMode.Region,
            Shortcut = _shortcut,
            UsePrintScreen = UsePrintScreenCheck.IsChecked == true,
            StartWithWindows = StartWithWindowsCheck.IsChecked == true
        };
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void OpenKeyboardSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("ms-settings:easeofaccess-keyboard")
        {
            UseShellExecute = true
        });
    }

    private void ShortcutInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ShortcutInput.Text = "Pressione a combinação…";
        ShortcutInput.SelectAll();
    }

    private void ShortcutInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        RecordShortcut(Keyboard.Modifiers, key);
        e.Handled = true;
    }

    private void ShortcutInput_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.PrintScreen)
        {
            RecordShortcut(Keyboard.Modifiers, key);
            e.Handled = true;
        }
    }

    private void RecordShortcut(ModifierKeys modifiers, Key key)
    {
        if (HotkeyService.TryCreateShortcutLabel(modifiers, key, out string shortcut))
        {
            _shortcut = shortcut;
            ShortcutInput.Text = shortcut;
            Keyboard.ClearFocus();
            return;
        }

        if (key is not (Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt))
        {
            ShortcutInput.Text = "Use uma combinação ou pressione Print Screen";
        }
    }

    private void ShortcutInput_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ShortcutInput.Text = _shortcut;
    }

    private nint WindowProcedure(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == WmHotkey &&
            wParam.ToInt32() == RecorderPrintScreenId &&
            ShortcutInput.IsKeyboardFocusWithin)
        {
            RecordShortcut(ModifierKeys.None, Key.PrintScreen);
            handled = true;
        }

        return nint.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);

    private sealed record ModeOption(CaptureMode Value, string Label);
}
