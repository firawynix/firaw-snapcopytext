using System.Windows;
using System.Windows.Input;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Views;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;

public partial class CaptureTargetPickerWindow : Window
{
    public CaptureTarget? SelectedTarget { get; private set; }

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
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e) => ConfirmSelection();

    private void TargetsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ConfirmSelection();

    private void ConfirmSelection()
    {
        if (TargetsList.SelectedItem is not CaptureTarget target)
        {
            return;
        }

        SelectedTarget = target;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
