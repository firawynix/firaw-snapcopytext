using System.Windows;
using System.Windows.Input;
using Firaw.SnapCopyText.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Firaw.SnapCopyText.Views;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;

public partial class CaptureTargetPickerWindow : Window
{
    public CaptureTarget? SelectedTarget { get; private set; }
    public CaptureResultAction SelectedAction { get; private set; } = CaptureResultAction.OpenEditor;

    public CaptureTargetPickerWindow(CaptureMode mode, IReadOnlyList<CaptureTarget> targets)
    {
        InitializeComponent();
        HeadingText.Text = mode == CaptureMode.Monitor ? "Escolha o monitor" : "Escolha a janela";
        HelpText.Text = mode == CaptureMode.Monitor
            ? "Selecione qual tela inteira deseja capturar."
            : "Selecione um dos programas abertos abaixo.";
        TargetsList.ItemsSource = targets;
        TargetsList.SelectedIndex = targets.Count > 0 ? 0 : -1;
        ConfirmButton.IsEnabled = targets.Count > 0;
        CopyButton.IsEnabled = targets.Count > 0;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e) =>
        ConfirmSelection(CaptureResultAction.OpenEditor);

    private void CopyButton_Click(object sender, RoutedEventArgs e) =>
        ConfirmSelection(CaptureResultAction.CopyImage);

    private void TargetsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        ConfirmSelection(CaptureResultAction.OpenEditor);

    private void ConfirmSelection(CaptureResultAction action)
    {
        if (TargetsList.SelectedItem is not CaptureTarget target)
        {
            return;
        }

        SelectedTarget = target;
        SelectedAction = action;
        DialogResult = true;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
        {
            ConfirmSelection(CaptureResultAction.CopyImage);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ConfirmSelection(CaptureResultAction.OpenEditor);
            e.Handled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
