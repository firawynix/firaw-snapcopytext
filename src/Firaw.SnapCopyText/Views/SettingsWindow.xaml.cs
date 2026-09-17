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
using TextBox = System.Windows.Controls.TextBox;

public partial class SettingsWindow : Window
{
    private const int RecorderPrintScreenIdBase = 0x5350;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkSnapshot = 0x2C;

    private string _shortcut;
    private string _printScreenShortcut;
    private string _altPrintScreenShortcut;
    private string _controlPrintScreenShortcut;
    private nint _windowHandle;
    private HwndSource? _windowSource;
    private readonly List<int> _registeredRecorderHotkeys = [];
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
        _printScreenShortcut = preferences.PrintScreenShortcut;
        _altPrintScreenShortcut = preferences.AltPrintScreenShortcut;
        _controlPrintScreenShortcut = preferences.ControlPrintScreenShortcut;
        ShortcutInput.Text = _shortcut;
        PrintScreenShortcutInput.Text = _printScreenShortcut;
        AltPrintScreenShortcutInput.Text = _altPrintScreenShortcut;
        ControlPrintScreenShortcutInput.Text = _controlPrintScreenShortcut;
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
        for (uint modifiers = 0; modifiers <= (ModAlt | ModControl | ModShift); modifiers++)
        {
            int id = RecorderPrintScreenIdBase + (int)modifiers;
            if (RegisterHotKey(_windowHandle, id, modifiers | ModNoRepeat, VkSnapshot))
            {
                _registeredRecorderHotkeys.Add(id);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        foreach (int id in _registeredRecorderHotkeys)
        {
            UnregisterHotKey(_windowHandle, id);
        }
        _windowSource?.RemoveHook(WindowProcedure);
        base.OnClosed(e);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateShortcutAssignments())
        {
            return;
        }

        SavedPreferences = new CapturePreferences
        {
            DefaultMode = DefaultModeCombo.SelectedValue is CaptureMode mode ? mode : CaptureMode.Region,
            Shortcut = _shortcut,
            UsePrintScreen = RegionFirawRadio.IsChecked == true,
            UseAltPrintScreen = MonitorFirawRadio.IsChecked == true,
            UseControlPrintScreen = WindowFirawRadio.IsChecked == true,
            PrintScreenShortcut = _printScreenShortcut,
            AltPrintScreenShortcut = _altPrintScreenShortcut,
            ControlPrintScreenShortcut = _controlPrintScreenShortcut,
            PrintScreenMode = SelectedMode(PrintScreenModeCombo, CaptureMode.Region),
            AltPrintScreenMode = SelectedMode(AltPrintScreenModeCombo, CaptureMode.Monitor),
            ControlPrintScreenMode = SelectedMode(ControlPrintScreenModeCombo, CaptureMode.Window),
            StartWithWindows = StartWithWindowsCheck.IsChecked == true
        };
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

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

    private void ProfileShortcutInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox input)
        {
            input.Text = "Pressione Print Screen…";
            input.SelectAll();
        }
    }

    private void ProfileShortcutInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        RecordProfileShortcut((TextBox)sender, Keyboard.Modifiers, key);
        e.Handled = true;
    }

    private void ProfileShortcutInput_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.PrintScreen)
        {
            RecordProfileShortcut((TextBox)sender, Keyboard.Modifiers, key);
            e.Handled = true;
        }
    }

    private void ProfileShortcutInput_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox input)
        {
            input.Text = GetProfileShortcut(input);
        }
    }

    private void RecordProfileShortcut(TextBox input, ModifierKeys modifiers, Key key)
    {
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt)
        {
            return;
        }

        if (key != Key.PrintScreen ||
            !HotkeyService.TryCreateShortcutLabel(modifiers, key, out string shortcut))
        {
            input.Text = "Use Print Screen";
            return;
        }

        SetProfileShortcut(input, shortcut);
        SetProfileEnabled(input);
        input.Text = shortcut;
        WindowsShortcutStatus.Text = $"Atalho alterado para {shortcut}. Clique em Salvar para aplicar.";
        Keyboard.ClearFocus();
    }

    private string GetProfileShortcut(TextBox input)
    {
        if (ReferenceEquals(input, PrintScreenShortcutInput)) return _printScreenShortcut;
        if (ReferenceEquals(input, AltPrintScreenShortcutInput)) return _altPrintScreenShortcut;
        return _controlPrintScreenShortcut;
    }

    private void SetProfileShortcut(TextBox input, string shortcut)
    {
        if (ReferenceEquals(input, PrintScreenShortcutInput)) _printScreenShortcut = shortcut;
        else if (ReferenceEquals(input, AltPrintScreenShortcutInput)) _altPrintScreenShortcut = shortcut;
        else _controlPrintScreenShortcut = shortcut;
    }

    private void SetProfileEnabled(TextBox input)
    {
        if (ReferenceEquals(input, PrintScreenShortcutInput)) RegionFirawRadio.IsChecked = true;
        else if (ReferenceEquals(input, AltPrintScreenShortcutInput)) MonitorFirawRadio.IsChecked = true;
        else WindowFirawRadio.IsChecked = true;
    }

    private bool ValidateShortcutAssignments()
    {
        List<string> enabled = [];
        if (RegionFirawRadio.IsChecked == true) enabled.Add(_printScreenShortcut);
        if (MonitorFirawRadio.IsChecked == true) enabled.Add(_altPrintScreenShortcut);
        if (WindowFirawRadio.IsChecked == true) enabled.Add(_controlPrintScreenShortcut);

        string? duplicate = enabled
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicate is not null)
        {
            WindowsShortcutStatus.Text = $"O atalho {duplicate} está repetido. Defina uma combinação diferente.";
            return false;
        }

        if (enabled.Contains(_shortcut, StringComparer.OrdinalIgnoreCase))
        {
            WindowsShortcutStatus.Text = $"O atalho adicional {_shortcut} já está sendo usado no perfil acima.";
            return false;
        }

        return true;
    }

    private static CaptureMode SelectedMode(System.Windows.Controls.ComboBox combo, CaptureMode fallback) =>
        combo.SelectedValue is CaptureMode mode ? mode : fallback;

    private nint WindowProcedure(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (TryDecodeRecorderHotkey(message, wParam, out ModifierKeys modifiers))
        {
            TextBox? profileInput = FocusedProfileShortcutInput();
            if (profileInput is not null)
            {
                RecordProfileShortcut(profileInput, modifiers, Key.PrintScreen);
                handled = true;
            }
            else if (ShortcutInput.IsKeyboardFocusWithin)
            {
                RecordShortcut(modifiers, Key.PrintScreen);
                handled = true;
            }
        }

        return nint.Zero;
    }

    internal static bool TryDecodeRecorderHotkey(int message, nint wParam, out ModifierKeys modifiers)
    {
        modifiers = ModifierKeys.None;
        if (message != WmHotkey)
        {
            return false;
        }

        // wParam is pointer-sized. Converting every Windows message with ToInt32
        // can overflow on x64 before we even know that the message is WM_HOTKEY.
        long rawId = wParam.ToInt64();
        long lastRecorderId = RecorderPrintScreenIdBase + (ModAlt | ModControl | ModShift);
        if (rawId < RecorderPrintScreenIdBase || rawId > lastRecorderId)
        {
            return false;
        }

        modifiers = ToModifierKeys((uint)(rawId - RecorderPrintScreenIdBase));
        return true;
    }

    private TextBox? FocusedProfileShortcutInput()
    {
        if (PrintScreenShortcutInput.IsKeyboardFocusWithin) return PrintScreenShortcutInput;
        if (AltPrintScreenShortcutInput.IsKeyboardFocusWithin) return AltPrintScreenShortcutInput;
        if (ControlPrintScreenShortcutInput.IsKeyboardFocusWithin) return ControlPrintScreenShortcutInput;
        return null;
    }

    private static ModifierKeys ToModifierKeys(uint modifiers)
    {
        ModifierKeys result = ModifierKeys.None;
        if ((modifiers & ModControl) != 0) result |= ModifierKeys.Control;
        if ((modifiers & ModShift) != 0) result |= ModifierKeys.Shift;
        if ((modifiers & ModAlt) != 0) result |= ModifierKeys.Alt;
        return result;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);

    private sealed record ModeOption(CaptureMode Value, string Label);
}
