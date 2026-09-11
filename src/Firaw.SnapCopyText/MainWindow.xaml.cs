using System.Windows;

namespace Firaw.SnapCopyText;

public partial class MainWindow : Window
{
    public event EventHandler? CaptureRequested;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetStatus(string message) => StatusText.Text = message;

    private void CaptureButton_Click(object sender, RoutedEventArgs e) =>
        CaptureRequested?.Invoke(this, EventArgs.Empty);
}
