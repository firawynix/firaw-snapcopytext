using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Button = System.Windows.Controls.Button;
using DragDeltaEventArgs = System.Windows.Controls.Primitives.DragDeltaEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Firaw.SnapCopyText.Views;

public partial class CaptureOverlayWindow : Window
{
    private const double HandleRadius = 6;
    private const double MinimumSelectionSize = 16;
    private readonly DesktopSnapshot _snapshot;
    private readonly CaptureService _captureService;
    private Point _start;
    private Rect _selection = Rect.Empty;
    private bool _drawing;

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
            UpdateDimming(Rect.Empty);
        };
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsInteractiveElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        _start = Clamp(e.GetPosition(Root));
        _selection = new Rect(_start, _start);
        _drawing = true;
        CaptureMouse();
        InstructionText.Text = "Solte para ajustar a região";
        UpdateSelectionVisuals(showControls: false);
        e.Handled = true;
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_drawing)
        {
            return;
        }

        _selection = CreateRect(_start, Clamp(e.GetPosition(Root)));
        UpdateSelectionVisuals(showControls: false);
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_drawing)
        {
            return;
        }

        _drawing = false;
        ReleaseMouseCapture();
        _selection = CreateRect(_start, Clamp(e.GetPosition(Root)));

        if (_selection.Width < MinimumSelectionSize || _selection.Height < MinimumSelectionSize)
        {
            ClearSelection();
            return;
        }

        InstructionText.Text = "Mova ou redimensione  •  Enter abre o editor  •  Esc cancela";
        UpdateSelectionVisuals(showControls: true);
        e.Handled = true;
    }

    private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_selection.IsEmpty)
        {
            return;
        }

        _selection = SelectionGeometry.Move(
            _selection,
            e.HorizontalChange,
            e.VerticalChange,
            SelectionBounds());
        UpdateSelectionVisuals(showControls: true);
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_selection.IsEmpty ||
            sender is not Thumb thumb ||
            !Enum.TryParse(thumb.Tag?.ToString(), out ResizeHandle handle))
        {
            return;
        }

        _selection = SelectionGeometry.Resize(
            _selection,
            handle,
            e.HorizontalChange,
            e.VerticalChange,
            SelectionBounds());
        UpdateSelectionVisuals(showControls: true);
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e) => ConfirmSelection();

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && !_selection.IsEmpty)
        {
            ConfirmSelection();
            e.Handled = true;
        }
    }

    private void ConfirmSelection()
    {
        if (_selection.IsEmpty ||
            _selection.Width < MinimumSelectionSize ||
            _selection.Height < MinimumSelectionSize)
        {
            return;
        }

        double scaleX = _snapshot.Image.PixelWidth / ActualWidth;
        double scaleY = _snapshot.Image.PixelHeight / ActualHeight;
        Int32Rect pixelRegion = CaptureService.NormalizeSelection(
            _selection.Left * scaleX,
            _selection.Top * scaleY,
            _selection.Right * scaleX,
            _selection.Bottom * scaleY,
            new Int32Rect(0, 0, _snapshot.Image.PixelWidth, _snapshot.Image.PixelHeight));

        SelectedImage = _captureService.Crop(_snapshot.Image, pixelRegion);
        DialogResult = true;
    }

    private void ClearSelection()
    {
        _selection = Rect.Empty;
        SelectionBorder.Visibility = Visibility.Collapsed;
        SizeBadge.Visibility = Visibility.Collapsed;
        ActionBar.Visibility = Visibility.Collapsed;
        SetControlVisibility(Visibility.Collapsed);
        InstructionText.Text = "Arraste para selecionar  •  Esc para cancelar";
        UpdateDimming(Rect.Empty);
    }

    private void UpdateSelectionVisuals(bool showControls)
    {
        Rect region = _selection;
        SelectionBorder.Visibility = Visibility.Visible;
        SizeBadge.Visibility = Visibility.Visible;

        SetRect(SelectionBorder, region.Left, region.Top, region.Width, region.Height);
        SetRect(MoveThumb, region.Left, region.Top, region.Width, region.Height);

        SizeText.Text = $"{Math.Round(region.Width)} × {Math.Round(region.Height)}";
        SizeBadge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double badgeLeft = Math.Clamp(region.Left, 0, Math.Max(0, ActualWidth - SizeBadge.DesiredSize.Width));
        double badgeTop = region.Top - SizeBadge.DesiredSize.Height - 8;
        if (badgeTop < 0)
        {
            badgeTop = Math.Min(ActualHeight - SizeBadge.DesiredSize.Height, region.Top + 8);
        }
        Canvas.SetLeft(SizeBadge, badgeLeft);
        Canvas.SetTop(SizeBadge, badgeTop);

        if (showControls)
        {
            SetControlVisibility(Visibility.Visible);
            PositionHandle(TopLeftHandle, region.Left, region.Top);
            PositionHandle(TopHandle, region.Left + (region.Width / 2), region.Top);
            PositionHandle(TopRightHandle, region.Right, region.Top);
            PositionHandle(RightHandle, region.Right, region.Top + (region.Height / 2));
            PositionHandle(BottomRightHandle, region.Right, region.Bottom);
            PositionHandle(BottomHandle, region.Left + (region.Width / 2), region.Bottom);
            PositionHandle(BottomLeftHandle, region.Left, region.Bottom);
            PositionHandle(LeftHandle, region.Left, region.Top + (region.Height / 2));
            PositionActionBar(region);
        }
        else
        {
            SetControlVisibility(Visibility.Collapsed);
        }

        UpdateDimming(region);
    }

    private void PositionActionBar(Rect region)
    {
        ActionBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double left = Math.Clamp(
            region.Right - ActionBar.DesiredSize.Width,
            0,
            Math.Max(0, ActualWidth - ActionBar.DesiredSize.Width));
        double top = region.Bottom + 10;
        if (top + ActionBar.DesiredSize.Height > ActualHeight)
        {
            top = Math.Max(0, region.Top - ActionBar.DesiredSize.Height - 10);
        }

        Canvas.SetLeft(ActionBar, left);
        Canvas.SetTop(ActionBar, top);
    }

    private void SetControlVisibility(Visibility visibility)
    {
        MoveThumb.Visibility = visibility;
        ActionBar.Visibility = visibility;
        TopLeftHandle.Visibility = visibility;
        TopHandle.Visibility = visibility;
        TopRightHandle.Visibility = visibility;
        RightHandle.Visibility = visibility;
        BottomRightHandle.Visibility = visibility;
        BottomHandle.Visibility = visibility;
        BottomLeftHandle.Visibility = visibility;
        LeftHandle.Visibility = visibility;
    }

    private static void PositionHandle(FrameworkElement handle, double x, double y)
    {
        Canvas.SetLeft(handle, x - HandleRadius);
        Canvas.SetTop(handle, y - HandleRadius);
    }

    private void UpdateDimming(Rect region)
    {
        double width = Math.Max(0, ActualWidth);
        double height = Math.Max(0, ActualHeight);

        if (region.IsEmpty)
        {
            SetRect(DimTop, 0, 0, width, height);
            SetRect(DimLeft, 0, 0, 0, 0);
            SetRect(DimRight, 0, 0, 0, 0);
            SetRect(DimBottom, 0, 0, 0, 0);
            return;
        }

        SetRect(DimTop, 0, 0, width, region.Top);
        SetRect(DimLeft, 0, region.Top, region.Left, region.Height);
        SetRect(DimRight, region.Right, region.Top, Math.Max(0, width - region.Right), region.Height);
        SetRect(DimBottom, 0, region.Bottom, width, Math.Max(0, height - region.Bottom));
    }

    private static void SetRect(FrameworkElement element, double left, double top, double width, double height)
    {
        Canvas.SetLeft(element, left);
        Canvas.SetTop(element, top);
        element.Width = Math.Max(0, width);
        element.Height = Math.Max(0, height);
    }

    private Rect SelectionBounds() => new(0, 0, Math.Max(0, ActualWidth), Math.Max(0, ActualHeight));

    private Point Clamp(Point point) => new(
        Math.Clamp(point.X, 0, ActualWidth),
        Math.Clamp(point.Y, 0, ActualHeight));

    private static Rect CreateRect(Point first, Point second) => new(
        new Point(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y)),
        new Point(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y)));

    private static bool IsInteractiveElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Thumb or Button)
            {
                return true;
            }
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }
}
