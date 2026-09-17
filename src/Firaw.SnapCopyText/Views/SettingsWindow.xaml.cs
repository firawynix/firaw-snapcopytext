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

        ModeOption[] modes =
        {
            new ModeOption(CaptureMode.Region, "Selecionar região"),
            new ModeOption(CaptureMode.Window, "Escolher janela"),
            new ModeOption(CaptureMode.Monitor, "Escolher monitor")
        };

        DefaultModeCombo.ItemsSource = modes;
        PrintScreenModeCombo.ItemsSource = modes;
        AltPrintScreenModeCombo.ItemsSource = modes;
        ControlPrintScreenModeCombo.ItemsSource = modes;
        DefaultModeCombo.SelectedValue = preferences.DefaultMode;
        PrintScreenModeCombo.SelectedValue = preferences.PrintScreenMode;
        AltPrintScreenModeCombo.SelectedValue = preferences.AltPrintScreenMode;
        ControlPrintScreenModeCombo.SelectedValue = preferences.ControlPrintScreenMode;
        _shortcut = preferences.Shortcut;
        ShortcutInput.Text = _shortcut;
        RegionFirawRadio.IsChecked = preferences.UsePrintScreen;
        RegionWindowsRadio.IsChecked = !preferences.UsePrintScreen;
        MonitorFirawRadio.IsChecked = preferences.UseAltPrintScreen;
        MonitorWindowsRadio.IsChecked = !preferences.UseAltPrintScreen;
        WindowFirawRadio.IsChecked = preferences.UseControlPrintScreen;
        WindowWindowsRadio.IsChecked = !preferences.UseControlPrintScreen;
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
            UsePrintScreen = RegionFirawRadio.IsChecked == true,
            UseAltPrintScreen = MonitorFirawRadio.IsChecked == true,
            UseControlPrintScreen = WindowFirawRadio.IsChecked == true,
            PrintScreenMode = SelectedMode(PrintScreenModeCombo, CaptureMode.Region),
            AltPrintScreenMode = SelectedMode(AltPrintScreenModeCombo, CaptureMode.Monitor),
            ControlPrintScreenMode = SelectedMode(ControlPrintScreenModeCombo, CaptureMode.Window),
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

    private void ApplyFirawPresetButton_Click(object sender, RoutedEventArgs e)
    {
        RegionFirawRadio.IsChecked = true;
        MonitorFirawRadio.IsChecked = true;
        WindowFirawRadio.IsChecked = true;
        WindowsShortcutStatus.Text = "Perfil Firaw selecionado. Clique em Salvar para aplicar.";
    }

    private void RestoreWindowsPresetButton_Click(object sender, RoutedEventArgs e)
    {
        RegionWindowsRadio.IsChecked = true;
        MonitorWindowsRadio.IsChecked = true;
        WindowWindowsRadio.IsChecked = true;
        WindowsShortcutStatus.Text = "As três combinações serão devolvidas ao Windows ao salvar.";
    }

    private void DisableWindowsSnippingButton_Click(object sender, RoutedEventArgs e) =>
        SetWindowsSnipping(enabled: false);

    private void EnableWindowsSnippingButton_Click(object sender, RoutedEventArgs e) =>
        SetWindowsSnipping(enabled: true);

    private void SetWindowsSnipping(bool enabled)
    {
        try
        {
            WindowsPrintScreenService.SetScreenSnippingEnabled(enabled);
            if (enabled)
            {
                RegionWindowsRadio.IsChecked = true;
            }
            else
            {
                RegionFirawRadio.IsChecked = true;
            }
            WindowsShortcutStatus.Text = enabled
                ? "O recorte nativo foi restaurado e Print Screen foi marcado como Windows. Clique em Salvar."
                : "O recorte nativo foi desativado e Print Screen foi marcado como Firaw. Clique em Salvar.";
        }
        catch (Exception exception)
        {
            WindowsShortcutStatus.Text = $"Não foi possível alterar o Windows: {exception.Message}";
        }
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
        ModifierKeys relevant = modifiers & (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt);
        if (key == Key.PrintScreen && relevant is ModifierKeys.None or ModifierKeys.Alt or ModifierKeys.Control)
        {
            if (relevant == ModifierKeys.None) RegionFirawRadio.IsChecked = true;
            if (relevant == ModifierKeys.Alt) MonitorFirawRadio.IsChecked = true;
            if (relevant == ModifierKeys.Control) WindowFirawRadio.IsChecked = true;
            ShortcutInput.Text = _shortcut;
            WindowsShortcutStatus.Text = "Combinação ativada no perfil Print Screen acima.";
            Keyboard.ClearFocus();
            return;
        }

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

    private static CaptureMode SelectedMode(System.Windows.Controls.ComboBox combo, CaptureMode fallback) =>
        combo.SelectedValue is CaptureMode mode ? mode : fallback;

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
