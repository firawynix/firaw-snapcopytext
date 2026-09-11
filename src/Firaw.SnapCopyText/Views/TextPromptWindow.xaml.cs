using System.Windows;

namespace Firaw.SnapCopyText.Views;

public partial class TextPromptWindow : Window
{
    public string ResultText => InputText.Text.Trim();

    public TextPromptWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => InputText.Focus();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(InputText.Text))
        {
            DialogResult = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
