using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Firaw.SnapCopyText.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Firaw.SnapCopyText.Views;

public partial class CaptureOverlayWindow : Window
{
    private readonly DesktopSnapshot _snapshot;
    private readonly CaptureService _captureService;
    private Point _start;
    private bool _selecting;

    public BitmapSource? SelectedImage { get; private set; }

    public CaptureOverlayWindow(DesktopSnapshot snapshot, CaptureService captureService)
    {
        InitializeComponent();
        _snapshot = snapshot;
        _captureService = captureService;

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
        DesktopImage.Source = snapshot.Image;

        Loaded += (_, _) =>
        {
            Activate();
            Focus();
            UpdateDimming(new Rect());
        };
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _start = Clamp(e.GetPosition(Root));
        _selecting = true;
        CaptureMouse();
        UpdateSelection(_start);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (_selecting)
        {
            UpdateSelection(Clamp(e.GetPosition(Root)));
        }
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_selecting)
        {
            return;
        }

        _selecting = false;
        ReleaseMouseCapture();
        Point end = Clamp(e.GetPosition(Root));
        Rect displayRegion = CreateRect(_start, end);
        if (displayRegion.Width < 2 || displayRegion.Height < 2)
        {
            DialogResult = false;
            return;
        }

        double scaleX = _snapshot.Image.PixelWidth / ActualWidth;
        double scaleY = _snapshot.Image.PixelHeight / ActualHeight;
        Int32Rect pixelRegion = CaptureService.NormalizeSelection(
            displayRegion.Left * scaleX,
            displayRegion.Top * scaleY,
            displayRegion.Right * scaleX,
            displayRegion.Bottom * scaleY,
            new Int32Rect(0, 0, _snapshot.Image.PixelWidth, _snapshot.Image.PixelHeight));

        SelectedImage = _captureService.Crop(_snapshot.Image, pixelRegion);
        DialogResult = true;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }

    private void UpdateSelection(Point current)
    {
        Rect region = CreateRect(_start, current);
        SelectionBorder.Visibility = Visibility.Visible;
        SizeBadge.Visibility = Visibility.Visible;

        Canvas.SetLeft(SelectionBorder, region.Left);
        Canvas.SetTop(SelectionBorder, region.Top);
        SelectionBorder.Width = region.Width;
        SelectionBorder.Height = region.Height;

        SizeText.Text = $"{Math.Round(region.Width)} × {Math.Round(region.Height)}";
        SizeBadge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double badgeTop = region.Bottom + 8;
        if (badgeTop + SizeBadge.DesiredSize.Height > ActualHeight)
        {
            badgeTop = Math.Max(0, region.Top - SizeBadge.DesiredSize.Height - 8);
        }

        Canvas.SetLeft(SizeBadge, Math.Min(region.Left, Math.Max(0, ActualWidth - SizeBadge.DesiredSize.Width)));
        Canvas.SetTop(SizeBadge, badgeTop);
        UpdateDimming(region);
    }

    private void UpdateDimming(Rect region)
    {
        double width = Math.Max(0, ActualWidth);
        double height = Math.Max(0, ActualHeight);
        double left = region.IsEmpty ? 0 : region.Left;
        double top = region.IsEmpty ? 0 : region.Top;
        double right = region.IsEmpty ? 0 : region.Right;
        double bottom = region.IsEmpty ? 0 : region.Bottom;

        SetRect(DimTop, 0, 0, width, top);
        SetRect(DimLeft, 0, top, left, Math.Max(0, bottom - top));
        SetRect(DimRight, right, top, Math.Max(0, width - right), Math.Max(0, bottom - top));
        SetRect(DimBottom, 0, bottom, width, Math.Max(0, height - bottom));

        if (region.IsEmpty)
        {
            SetRect(DimTop, 0, 0, width, height);
        }
    }

    private static void SetRect(FrameworkElement element, double left, double top, double width, double height)
    {
        Canvas.SetLeft(element, left);
        Canvas.SetTop(element, top);
        element.Width = width;
        element.Height = height;
    }

    private Point Clamp(Point point) => new(
        Math.Clamp(point.X, 0, ActualWidth),
        Math.Clamp(point.Y, 0, ActualHeight));

    private static Rect CreateRect(Point first, Point second) => new(
        new Point(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y)),
        new Point(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y)));
}
