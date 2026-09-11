using System.Windows;

namespace Firaw.SnapCopyText.Views;

public partial class OcrPreviewWindow : Window
{
    public string ResultText => (RecognizedText.SelectionLength > 0
        ? RecognizedText.SelectedText
        : RecognizedText.Text).Trim();

    public OcrPreviewWindow(string text)
    {
        InitializeComponent();
        RecognizedText.Text = text;
        Loaded += (_, _) =>
        {
            RecognizedText.Focus();
            RecognizedText.CaretIndex = 0;
        };
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        RecognizedText.Focus();
        RecognizedText.SelectAll();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
