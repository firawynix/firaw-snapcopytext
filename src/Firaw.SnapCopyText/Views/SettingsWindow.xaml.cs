using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;

namespace Firaw.SnapCopyText.Views;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

public partial class SettingsWindow : Window
{
    private string _shortcut;
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
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SavedPreferences = new CapturePreferences
        {
            DefaultMode = DefaultModeCombo.SelectedValue is CaptureMode mode ? mode : CaptureMode.Region,
            Shortcut = _shortcut,
            UsePrintScreen = UsePrintScreenCheck.IsChecked == true
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
        if (HotkeyService.TryCreateShortcutLabel(Keyboard.Modifiers, key, out string shortcut))
        {
            _shortcut = shortcut;
            ShortcutInput.Text = shortcut;
            Keyboard.ClearFocus();
        }
        else if (key is not (Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt))
        {
            ShortcutInput.Text = "Use Ctrl, Shift ou Alt + uma tecla";
        }

        e.Handled = true;
    }

    private void ShortcutInput_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ShortcutInput.Text = _shortcut;
    }

    private sealed record ModeOption(CaptureMode Value, string Label);
}
