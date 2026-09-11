using System.Windows;
using System.Windows.Media.Imaging;

namespace Firaw.SnapCopyText.Views;

public partial class EditorWindow : Window
{
    public BitmapSource OriginalImage { get; }

    public EditorWindow(BitmapSource image)
    {
        InitializeComponent();
        OriginalImage = image;
        CaptureImage.Source = image;
        EditorSurface.Width = image.PixelWidth;
        EditorSurface.Height = image.PixelHeight;
    }
}
