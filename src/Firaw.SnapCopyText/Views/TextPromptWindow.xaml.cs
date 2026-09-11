using System.Windows;

namespace Firaw.SnapCopyText.Views;

public partial class TextPromptWindow : Window
{
    public string ResultText => InputText.Text.Trim();

    public TextPromptWindow(string heading = "Digite o texto")
    {
        InitializeComponent();
        PromptTitle.Text = heading;
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
