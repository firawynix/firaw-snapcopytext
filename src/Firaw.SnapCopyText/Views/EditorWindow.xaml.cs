using System.IO;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using DragCompletedEventArgs = System.Windows.Controls.Primitives.DragCompletedEventArgs;
using DragDeltaEventArgs = System.Windows.Controls.Primitives.DragDeltaEventArgs;
using DragStartedEventArgs = System.Windows.Controls.Primitives.DragStartedEventArgs;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.MessageBox;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextDataFormat = System.Windows.TextDataFormat;
using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;

namespace Firaw.SnapCopyText.Views;

public partial class EditorWindow : Window
{
    private const double CaptureGap = 24;
    private const double MinimumCaptureWidth = 48;
    private const double MinimumCaptureHeight = 36;
    private static readonly object CaptureAnnotationTag = new();
    private readonly AnnotationHistory<EditorAction> _history = new();
    private readonly OcrService _ocrService = new();
    private readonly CaptureService _captureService = new();
    private readonly SensitiveDataDetector _sensitiveDataDetector = new();
    private readonly TextHistoryService _textHistory = TextHistoryService.Shared;
    private readonly HashSet<UIElement> _selectedElements = [];
    private readonly Dictionary<UIElement, Geometry?> _eraserOriginalClips = [];
    private readonly List<Thumb> _resizeHandles = [];
    private EditorTool _currentTool = EditorTool.Select;
    private EraserMode _eraserMode = EraserMode.Circle;
    private Point _startPoint;
    private Point _lastPoint;
    private Vector _moveDelta;
    private UIElement? _draft;
    private Rectangle? _textSelectionBox;
    private Rectangle? _selectionOutline;
    private Shape? _eraserPreview;
    private bool _drawing;
    private bool _marqueeSelecting;
    private bool _movingSelection;
    private bool _erasing;
    private bool _selectingTextRegion;
    private bool _textSelectionMode;
    private string _currentColor = "#19D3E6";
    private UIElement? _transformingElement;
    private Matrix _transformStartMatrix;
    private Point _rotationStartPoint;
    private Point _rotationCenter;
    private bool _rotating;

    public BitmapSource OriginalImage { get; }

    public EditorWindow(BitmapSource image)
    {
        InitializeComponent();
        OriginalImage = image;
        EditorSurface.Width = image.PixelWidth;
        EditorSurface.Height = image.PixelHeight;
        AddCaptureAnnotation(image, new Point(0, 0), recordHistory: false);
        Title = $"Firaw - SnapCopyText • Editor • {image.PixelWidth} × {image.PixelHeight}";
        ConfigureInitialWindowSize();
        CopiedTextList.ItemsSource = _textHistory.Items;
        _textHistory.Items.CollectionChanged += TextHistory_CollectionChanged;
        Closed += (_, _) => _textHistory.Items.CollectionChanged -= TextHistory_CollectionChanged;
        Loaded += (_, _) => FitEditorToCapture();
        UpdateTextHistoryUi();
    }

    private void ConfigureInitialWindowSize()
    {
        double maximumWidth = Math.Max(MinWidth, SystemParameters.WorkArea.Width - 48);
        double maximumHeight = Math.Max(MinHeight, SystemParameters.WorkArea.Height - 48);
        Width = Math.Clamp(OriginalImage.PixelWidth + 72, MinWidth, maximumWidth);
        Height = Math.Clamp(OriginalImage.PixelHeight + 190, MinHeight, maximumHeight);
    }

    private void FitEditorToCapture()
    {
        UpdateLayout();

        double compositionWidth = Math.Max(1, EditorSurface.Width);
        double compositionHeight = Math.Max(1, EditorSurface.Height);

        double scale = Math.Min(
            1,
            Math.Min(
                EditorScrollViewer.ViewportWidth / compositionWidth,
                EditorScrollViewer.ViewportHeight / compositionHeight));
        EditorFrame.LayoutTransform = scale < 0.999
            ? new ScaleTransform(scale, scale)
            : Transform.Identity;

        int captureCount = ActiveCaptureCount();
        string label = captureCount == 1
            ? $"Recorte exato: {compositionWidth:0} × {compositionHeight:0}"
            : $"{captureCount} capturas • composição {compositionWidth:0} × {compositionHeight:0}";
        if (scale < 0.999)
        {
            label += $" • visualização {scale:P0}";
        }

        Title = $"Firaw - SnapCopyText • Editor • {compositionWidth:0} × {compositionHeight:0}";
        EditorStatus.Text = label;
    }

    private void ToolButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton selected || ToolsPanel is null || EditorStatus is null)
        {
            return;
        }

        CancelTextSelection();

        foreach (ToggleButton button in ToolsPanel.Children.OfType<ToggleButton>())
        {
            if (!ReferenceEquals(button, selected))
            {
                button.IsChecked = false;
            }
        }

        if (Enum.TryParse(selected.Tag?.ToString(), out EditorTool tool))
        {
            _currentTool = tool;
            EditorStatus.Text = $"Ferramenta: {selected.Content}";
            EraserOptionsPanel.Visibility = tool == EditorTool.Eraser
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (tool != EditorTool.Select)
            {
                ClearSelection();
            }
        }
    }

    private void AnnotationCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = ClampToCanvas(e.GetPosition(AnnotationCanvas));
        _lastPoint = _startPoint;

        if (_textSelectionMode)
        {
            BeginTextRegionSelection();
            return;
        }

        if (_currentTool == EditorTool.Select)
        {
            BeginAnnotationSelection(e.OriginalSource as DependencyObject);
            e.Handled = true;
            return;
        }

        if (_currentTool == EditorTool.Eraser)
        {
            BeginErasing();
            e.Handled = true;
            return;
        }

        if (_currentTool is EditorTool.Text or EditorTool.Note)
        {
            AddText(_startPoint, asNote: _currentTool == EditorTool.Note);
            return;
        }

        _drawing = true;
        AnnotationCanvas.CaptureMouse();
        _draft = CreateDraft(_startPoint);
        AnnotationCanvas.Children.Add(_draft);
    }

    private void AnnotationCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        Point current = ClampToCanvas(e.GetPosition(AnnotationCanvas));

        if (_selectingTextRegion && _textSelectionBox is not null)
        {
            UpdateRectangle(_textSelectionBox, _startPoint, current);
            return;
        }

        if (_movingSelection)
        {
            MoveSelection(current);
            return;
        }

        if (_marqueeSelecting)
        {
            UpdateMarqueeSelection(current);
            return;
        }

        if (_erasing)
        {
            UpdateEraserPreview(current);
            EraseBetween(_lastPoint, current);
            _lastPoint = current;
            return;
        }

        if (!_drawing || _draft is null)
        {
            return;
        }

        UpdateDraft(_draft, _startPoint, current);
    }

    private async void AnnotationCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectingTextRegion)
        {
            await CompleteTextRegionSelectionAsync(ClampToCanvas(e.GetPosition(AnnotationCanvas)));
            return;
        }

        if (_movingSelection)
        {
            CompleteSelectionMove();
            return;
        }

        if (_marqueeSelecting)
        {
            CompleteMarqueeSelection(ClampToCanvas(e.GetPosition(AnnotationCanvas)));
            return;
        }

        if (_erasing)
        {
            Point eraseEnd = ClampToCanvas(e.GetPosition(AnnotationCanvas));
            EraseBetween(_lastPoint, eraseEnd);
            CompleteErasing();
            return;
        }

        if (!_drawing || _draft is null)
        {
            return;
        }

        _drawing = false;
        AnnotationCanvas.ReleaseMouseCapture();
        Point end = ClampToCanvas(e.GetPosition(AnnotationCanvas));
        UpdateDraft(_draft, _startPoint, end);

        if (_currentTool != EditorTool.Pencil && _currentTool != EditorTool.Highlight &&
            Math.Abs(end.X - _startPoint.X) < 2 && Math.Abs(end.Y - _startPoint.Y) < 2)
        {
            AnnotationCanvas.Children.Remove(_draft);
        }
        else if (_currentTool == EditorTool.Blur)
        {
            AnnotationCanvas.Children.Remove(_draft);
            UIElement blurred = CreateBlurAnnotation(CreateRect(_startPoint, end));
            AnnotationCanvas.Children.Add(blurred);
            RecordAddedAnnotations([blurred]);
            EditorStatus.Text = "Área borrada. Use Selecionar para mover depois.";
        }
        else
        {
            RecordAddedAnnotations([_draft]);
        }

        _draft = null;
        UpdateHistoryButtons();
    }

    private void BeginAnnotationSelection(DependencyObject? originalSource)
    {
        UIElement? hit = GetDirectAnnotation(originalSource);
        bool additive = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (hit is not null)
        {
            if (!additive && !_selectedElements.Contains(hit))
            {
                ClearSelection();
            }

            _selectedElements.Add(hit);
            _movingSelection = true;
            _moveDelta = default;
            AnnotationCanvas.CaptureMouse();
            UpdateSelectionOutline();
            AnnotationCanvas.Cursor = Cursors.SizeAll;
            EditorStatus.Text = _selectedElements.Count == 1
                ? "Objeto selecionado. Arraste para mover."
                : $"{_selectedElements.Count} objetos selecionados. Arraste para mover juntos.";
            return;
        }

        Rect selectedBounds = GetSelectionBounds();
        if (_selectedElements.Count > 0 && selectedBounds.Contains(_startPoint))
        {
            _movingSelection = true;
            _moveDelta = default;
            AnnotationCanvas.CaptureMouse();
            AnnotationCanvas.Cursor = Cursors.SizeAll;
            return;
        }

        if (!additive)
        {
            ClearSelection();
        }

        _marqueeSelecting = true;
        AnnotationCanvas.CaptureMouse();
        _selectionOutline = CreateSelectionRectangle(fillSelection: true);
        SelectionCanvas.Children.Add(_selectionOutline);
        UpdateRectangle(_selectionOutline, _startPoint, _startPoint);
        EditorStatus.Text = "Arraste uma área para selecionar um ou vários objetos.";
    }

    private void MoveSelection(Point current)
    {
        Vector requested = current - _lastPoint;
        Vector allowed = ClampMoveToCanvas(requested);
        if (Math.Abs(allowed.X) < 0.01 && Math.Abs(allowed.Y) < 0.01)
        {
            return;
        }

        TranslateAnnotations(_selectedElements, allowed);
        _moveDelta += allowed;
        _lastPoint += allowed;
        UpdateSelectionOutline();
    }

    private Vector ClampMoveToCanvas(Vector requested)
    {
        Rect bounds = GetSelectionBounds();
        if (bounds.IsEmpty)
        {
            return default;
        }

        double x = Math.Clamp(requested.X, -bounds.Left, AnnotationCanvas.ActualWidth - bounds.Right);
        double y = Math.Clamp(requested.Y, -bounds.Top, AnnotationCanvas.ActualHeight - bounds.Bottom);
        return new Vector(x, y);
    }

    private void CompleteSelectionMove()
    {
        _movingSelection = false;
        AnnotationCanvas.ReleaseMouseCapture();
        AnnotationCanvas.Cursor = Cursors.Arrow;

        if (_moveDelta.Length > 0.1)
        {
            UIElement[] moved = [.. _selectedElements];
            Vector delta = _moveDelta;
            _history.Add(new EditorAction(
                () => TranslateAnnotations(moved, -delta),
                () => TranslateAnnotations(moved, delta)));
            EditorStatus.Text = $"{moved.Length} objeto(s) movido(s).";
            UpdateHistoryButtons();
        }

        _moveDelta = default;
        UpdateSelectionOutline();
    }

    private async void AddCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            !Enum.TryParse(element.Tag?.ToString(), out CaptureMode mode) ||
            System.Windows.Application.Current.MainWindow is not MainWindow mainWindow)
        {
            return;
        }

        CancelTextSelection();
        ClearSelection();
        EditorStatus.Text = $"Escolha a nova captura de {CaptureModeLabel(mode).ToLowerInvariant()}.";
        BitmapSource? captured = await mainWindow.CaptureForEditorAsync(mode, this);
        if (captured is null)
        {
            EditorStatus.Text = "Nova captura cancelada.";
            return;
        }

        double right = AnnotationCanvas.Children
            .OfType<UIElement>()
            .Where(IsCaptureAnnotation)
            .Select(GetAnnotationBounds)
            .Where(bounds => !bounds.IsEmpty)
            .Select(bounds => bounds.Right)
            .DefaultIfEmpty(0)
            .Max();
        Point position = new(right > 0 ? right + CaptureGap : 0, 0);
        UIElement capture = AddCaptureAnnotation(captured, position, recordHistory: true);
        _selectedElements.Add(capture);
        UpdateSelectionOutline();
        FitEditorToCapture();
        EditorStatus.Text = $"{CaptureModeLabel(mode)} adicionada. Arraste para mover ou use as alças cianas para redimensionar.";
    }

    private UIElement AddCaptureAnnotation(BitmapSource source, Point position, bool recordHistory)
    {
        var capture = new Image
        {
            Source = source,
            Width = source.PixelWidth,
            Height = source.PixelHeight,
            Stretch = Stretch.Fill,
            Tag = CaptureAnnotationTag
        };
        Canvas.SetLeft(capture, position.X);
        Canvas.SetTop(capture, position.Y);

        double previousWidth = EditorSurface.Width;
        double previousHeight = EditorSurface.Height;
        double requiredWidth = Math.Max(previousWidth, position.X + capture.Width);
        double requiredHeight = Math.Max(previousHeight, position.Y + capture.Height);
        AnnotationCanvas.Children.Insert(ActiveCaptureCount(), capture);
        SetCompositionSize(requiredWidth, requiredHeight);

        if (recordHistory)
        {
            _history.Add(new EditorAction(
                () =>
                {
                    AnnotationCanvas.Children.Remove(capture);
                    _selectedElements.Remove(capture);
                    SetCompositionSize(previousWidth, previousHeight);
                    UpdateSelectionOutline();
                    FitEditorToCapture();
                },
                () =>
                {
                    if (!AnnotationCanvas.Children.Contains(capture))
                    {
                        AnnotationCanvas.Children.Insert(ActiveCaptureCount(), capture);
                    }
                    SetCompositionSize(requiredWidth, requiredHeight);
                    FitEditorToCapture();
                }));
            UpdateHistoryButtons();
        }

        return capture;
    }

    private void SetCompositionSize(double width, double height)
    {
        EditorSurface.Width = Math.Max(1, width);
        EditorSurface.Height = Math.Max(1, height);
    }

    private int ActiveCaptureCount() => AnnotationCanvas.Children
        .OfType<UIElement>()
        .Count(IsCaptureAnnotation);

    private static bool IsCaptureAnnotation(UIElement element) =>
        element is FrameworkElement frameworkElement &&
        ReferenceEquals(frameworkElement.Tag, CaptureAnnotationTag);

    private static string CaptureModeLabel(CaptureMode mode) => mode switch
    {
        CaptureMode.Window => "Janela",
        CaptureMode.Monitor => "Monitor",
        _ => "Região"
    };

    private void BringCaptureForwardButton_Click(object sender, RoutedEventArgs e) =>
        MoveSelectedCaptureToLayer(front: true);

    private void SendCaptureBackwardButton_Click(object sender, RoutedEventArgs e) =>
        MoveSelectedCaptureToLayer(front: false);

    private void MoveSelectedCaptureToLayer(bool front)
    {
        if (_selectedElements.Count != 1 ||
            _selectedElements.Single() is not UIElement capture ||
            !IsCaptureAnnotation(capture))
        {
            EditorStatus.Text = "Selecione um único print para alterar sua camada.";
            return;
        }

        int previousIndex = AnnotationCanvas.Children.IndexOf(capture);
        int targetIndex = front ? ActiveCaptureCount() - 1 : 0;
        if (previousIndex == targetIndex)
        {
            EditorStatus.Text = front
                ? "Este print já está na frente dos outros."
                : "Este print já está atrás dos outros.";
            return;
        }

        MoveCaptureToIndex(capture, targetIndex);
        _history.Add(new EditorAction(
            () => MoveCaptureToIndex(capture, previousIndex),
            () => MoveCaptureToIndex(capture, targetIndex)));
        UpdateHistoryButtons();
        EditorStatus.Text = front
            ? "Print trazido para a frente."
            : "Print enviado para trás.";
    }

    private void MoveCaptureToIndex(UIElement capture, int index)
    {
        if (!AnnotationCanvas.Children.Contains(capture))
        {
            return;
        }

        AnnotationCanvas.Children.Remove(capture);
        int captureIndex = Math.Clamp(index, 0, ActiveCaptureCount());
        AnnotationCanvas.Children.Insert(captureIndex, capture);
        UpdateSelectionOutline();
    }

    private void FitOutputButton_Click(object sender, RoutedEventArgs e)
    {
        UIElement[] elements = AnnotationCanvas.Children
            .OfType<UIElement>()
            .Where(element => element.Visibility == Visibility.Visible)
            .ToArray();
        Rect contentBounds = Rect.Empty;
        foreach (UIElement element in elements)
        {
            Rect bounds = GetAnnotationBounds(element);
            if (!bounds.IsEmpty)
            {
                contentBounds.Union(bounds);
            }
        }

        if (contentBounds.IsEmpty)
        {
            EditorStatus.Text = "Não há conteúdo para ajustar.";
            return;
        }

        double previousWidth = EditorSurface.Width;
        double previousHeight = EditorSurface.Height;
        Vector offset = new(-contentBounds.Left, -contentBounds.Top);
        double fittedWidth = Math.Max(1, Math.Ceiling(contentBounds.Width));
        double fittedHeight = Math.Max(1, Math.Ceiling(contentBounds.Height));
        bool sameSize = Math.Abs(previousWidth - fittedWidth) < 0.01 &&
                        Math.Abs(previousHeight - fittedHeight) < 0.01;
        if (offset.Length < 0.01 && sameSize)
        {
            EditorStatus.Text = "A saída já está ajustada ao conteúdo.";
            return;
        }

        ApplyOutputFit(elements, offset, fittedWidth, fittedHeight);
        _history.Add(new EditorAction(
            () => ApplyOutputFit(elements, -offset, previousWidth, previousHeight),
            () => ApplyOutputFit(elements, offset, fittedWidth, fittedHeight)));
        UpdateHistoryButtons();
        EditorStatus.Text = $"Saída ajustada para {fittedWidth:0} × {fittedHeight:0}.";
    }

    private void ApplyOutputFit(
        IEnumerable<UIElement> elements,
        Vector offset,
        double width,
        double height)
    {
        TranslateAnnotations(elements.Where(AnnotationCanvas.Children.Contains), offset);
        SetCompositionSize(width, height);
        UpdateSelectionOutline();
        FitEditorToCapture();
    }

    private void UpdateMarqueeSelection(Point current)
    {
        if (_selectionOutline is null)
        {
            return;
        }

        UpdateRectangle(_selectionOutline, _startPoint, current);
    }

    private void CompleteMarqueeSelection(Point end)
    {
        _marqueeSelecting = false;
        AnnotationCanvas.ReleaseMouseCapture();
        Rect region = CreateRect(_startPoint, end);

        foreach (UIElement annotation in AnnotationCanvas.Children)
        {
            if (GetAnnotationBounds(annotation).IntersectsWith(region))
            {
                _selectedElements.Add(annotation);
            }
        }

        RemoveSelectionVisual();
        UpdateSelectionOutline();
        EditorStatus.Text = _selectedElements.Count == 0
            ? "Nenhum objeto encontrado nessa área."
            : $"{_selectedElements.Count} objeto(s) selecionado(s). Arraste para mover.";
    }

    private void ClearSelection()
    {
        _selectedElements.Clear();
        RemoveSelectionVisual();
    }

    private void RemoveSelectionVisual()
    {
        if (_selectionOutline is not null)
        {
            SelectionCanvas.Children.Remove(_selectionOutline);
            _selectionOutline = null;
        }

        foreach (Thumb handle in _resizeHandles)
        {
            SelectionCanvas.Children.Remove(handle);
        }
        _resizeHandles.Clear();
    }

    private void UpdateSelectionOutline()
    {
        RemoveSelectionVisual();
        Rect bounds = GetSelectionBounds();
        if (bounds.IsEmpty)
        {
            return;
        }

        _selectionOutline = CreateSelectionRectangle(fillSelection: false);
        SelectionCanvas.Children.Add(_selectionOutline);
        PositionSelectionVisuals(bounds);

        if (_selectedElements.Count == 1)
        {
            AddTransformHandles(bounds);
        }
    }

    private Rectangle CreateSelectionRectangle(bool fillSelection) => new()
    {
        Stroke = (Brush)FindResource("FirawCyan"),
        StrokeThickness = 2,
        StrokeDashArray = new DoubleCollection { 5, 3 },
        IsHitTestVisible = false,
        Fill = fillSelection
            ? new SolidColorBrush(Color.FromArgb(28, 25, 211, 230))
            : Brushes.Transparent
    };

    private void AddTransformHandles(Rect bounds)
    {
        foreach (ResizeHandle resizeHandle in new[]
                 {
                     ResizeHandle.TopLeft,
                     ResizeHandle.TopRight,
                     ResizeHandle.BottomRight,
                     ResizeHandle.BottomLeft
                 })
        {
            var handle = new Thumb
            {
                Width = 14,
                Height = 14,
                Background = (Brush)FindResource("FirawCyan"),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                Cursor = resizeHandle is ResizeHandle.TopLeft or ResizeHandle.BottomRight
                    ? Cursors.SizeNWSE
                    : Cursors.SizeNESW,
                Tag = resizeHandle
            };
            handle.DragStarted += Transform_DragStarted;
            handle.DragDelta += Resize_DragDelta;
            handle.DragCompleted += Transform_DragCompleted;
            _resizeHandles.Add(handle);
            SelectionCanvas.Children.Add(handle);
        }

        var rotationHandle = new Thumb
        {
            Width = 16,
            Height = 16,
            Background = Brushes.Orange,
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Tag = "Rotate",
            ToolTip = "Arraste para girar"
        };
        rotationHandle.DragStarted += Transform_DragStarted;
        rotationHandle.DragDelta += Rotate_DragDelta;
        rotationHandle.DragCompleted += Transform_DragCompleted;
        _resizeHandles.Add(rotationHandle);
        SelectionCanvas.Children.Add(rotationHandle);

        PositionResizeHandles(bounds);
    }

    private void PositionSelectionVisuals(Rect bounds)
    {
        if (_selectionOutline is not null)
        {
            Canvas.SetLeft(_selectionOutline, bounds.Left - 3);
            Canvas.SetTop(_selectionOutline, bounds.Top - 3);
            _selectionOutline.Width = bounds.Width + 6;
            _selectionOutline.Height = bounds.Height + 6;
        }
        PositionResizeHandles(bounds);
    }

    private void PositionResizeHandles(Rect bounds)
    {
        foreach (Thumb handle in _resizeHandles)
        {
            if (handle.Tag is "Rotate")
            {
                Canvas.SetLeft(handle, bounds.Left + bounds.Width / 2 - handle.Width / 2);
                Canvas.SetTop(handle, Math.Max(0, bounds.Top - 30));
                continue;
            }

            if (handle.Tag is not ResizeHandle resizeHandle)
            {
                continue;
            }

            Point point = resizeHandle switch
            {
                ResizeHandle.TopLeft => bounds.TopLeft,
                ResizeHandle.TopRight => bounds.TopRight,
                ResizeHandle.BottomRight => bounds.BottomRight,
                _ => bounds.BottomLeft
            };
            Canvas.SetLeft(handle, point.X - handle.Width / 2);
            Canvas.SetTop(handle, point.Y - handle.Height / 2);
        }
    }

    private void Transform_DragStarted(object sender, DragStartedEventArgs e)
    {
        _transformingElement = _selectedElements.Count == 1
            ? _selectedElements.Single()
            : null;
        if (_transformingElement is null)
        {
            return;
        }

        _transformStartMatrix = _transformingElement.RenderTransform.Value;
        _rotating = sender is Thumb { Tag: "Rotate" };
        Rect bounds = GetAnnotationBounds(_transformingElement);
        _rotationCenter = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        _rotationStartPoint = Mouse.GetPosition(AnnotationCanvas);
    }

    private void Resize_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_transformingElement is null || sender is not Thumb { Tag: ResizeHandle handle })
        {
            return;
        }

        Rect bounds = GetAnnotationBounds(_transformingElement);
        if (bounds.Width < 0.01 || bounds.Height < 0.01)
        {
            return;
        }

        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double minimumWidth = IsCaptureAnnotation(_transformingElement)
            ? MinimumCaptureWidth
            : Math.Min(12, Math.Max(1, bounds.Width));
        double minimumHeight = IsCaptureAnnotation(_transformingElement)
            ? MinimumCaptureHeight
            : Math.Min(12, Math.Max(1, bounds.Height));

        if (handle is ResizeHandle.TopLeft or ResizeHandle.BottomLeft)
        {
            left = Math.Clamp(left + e.HorizontalChange, 0, Math.Max(0, right - minimumWidth));
        }
        else
        {
            right = Math.Max(left + minimumWidth, right + e.HorizontalChange);
        }

        if (handle is ResizeHandle.TopLeft or ResizeHandle.TopRight)
        {
            top = Math.Clamp(top + e.VerticalChange, 0, Math.Max(0, bottom - minimumHeight));
        }
        else
        {
            bottom = Math.Max(top + minimumHeight, bottom + e.VerticalChange);
        }

        Rect resized = new(left, top, right - left, bottom - top);
        double canvasLeft = GetCanvasOffset(_transformingElement, Canvas.LeftProperty);
        double canvasTop = GetCanvasOffset(_transformingElement, Canvas.TopProperty);
        Point fixedCorner = handle switch
        {
            ResizeHandle.TopLeft => bounds.BottomRight,
            ResizeHandle.TopRight => bounds.BottomLeft,
            ResizeHandle.BottomRight => bounds.TopLeft,
            _ => bounds.TopRight
        };
        Matrix transform = _transformingElement.RenderTransform.Value;
        transform.ScaleAt(resized.Width / bounds.Width, resized.Height / bounds.Height,
            fixedCorner.X - canvasLeft, fixedCorner.Y - canvasTop);
        ApplyAnnotationTransform(_transformingElement, transform);
    }

    private void Rotate_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_transformingElement is null || !_rotating)
        {
            return;
        }

        Point current = Mouse.GetPosition(AnnotationCanvas);
        Vector start = _rotationStartPoint - _rotationCenter;
        Vector end = current - _rotationCenter;
        if (start.Length < 1 || end.Length < 1)
        {
            return;
        }

        double angle = Math.Atan2(end.Y, end.X) - Math.Atan2(start.Y, start.X);
        Matrix transform = _transformStartMatrix;
        transform.RotateAt(angle * 180 / Math.PI,
            _rotationCenter.X - GetCanvasOffset(_transformingElement, Canvas.LeftProperty),
            _rotationCenter.Y - GetCanvasOffset(_transformingElement, Canvas.TopProperty));
        ApplyAnnotationTransform(_transformingElement, transform);
    }

    private void Transform_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_transformingElement is null)
        {
            return;
        }

        UIElement element = _transformingElement;
        Matrix before = _transformStartMatrix;
        Matrix after = element.RenderTransform.Value;
        _transformingElement = null;
        _rotating = false;

        if (before != after)
        {
            _history.Add(new EditorAction(
                () => ApplyAnnotationTransform(element, before),
                () => ApplyAnnotationTransform(element, after)));
            UpdateHistoryButtons();
        }

        UpdateSelectionOutline();
        FitEditorToCapture();
        EditorStatus.Text = "Tamanho/rotação atualizado. Ctrl+Z para desfazer.";
    }

    private static double GetCanvasOffset(UIElement element, DependencyProperty property)
    {
        double value = (double)element.GetValue(property);
        return double.IsNaN(value) ? 0 : value;
    }

    private void ApplyAnnotationTransform(UIElement element, Matrix transform)
    {
        element.RenderTransform = new MatrixTransform(transform);
        Rect bounds = GetAnnotationBounds(element);
        SetCompositionSize(
            Math.Max(EditorSurface.Width, bounds.Right),
            Math.Max(EditorSurface.Height, bounds.Bottom));
        PositionSelectionVisuals(bounds);
    }

    private Rect GetSelectionBounds()
    {
        Rect bounds = Rect.Empty;
        foreach (UIElement element in _selectedElements.Where(AnnotationCanvas.Children.Contains))
        {
            Rect itemBounds = GetAnnotationBounds(element);
            if (!itemBounds.IsEmpty)
            {
                bounds.Union(itemBounds);
            }
        }

        return bounds;
    }

    private Rect GetAnnotationBounds(UIElement element)
    {
        try
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(element);
            if ((bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0) &&
                element is FrameworkElement frameworkElement)
            {
                double width = frameworkElement.ActualWidth > 0
                    ? frameworkElement.ActualWidth
                    : frameworkElement.Width;
                double height = frameworkElement.ActualHeight > 0
                    ? frameworkElement.ActualHeight
                    : frameworkElement.Height;
                bounds = new Rect(0, 0, Math.Max(0, width), Math.Max(0, height));
            }

            return element.TransformToAncestor(AnnotationCanvas).TransformBounds(bounds);
        }
        catch (InvalidOperationException)
        {
            return Rect.Empty;
        }
    }

    private UIElement? GetDirectAnnotation(DependencyObject? source)
    {
        DependencyObject? current = source;
        while (current is not null && !ReferenceEquals(current, AnnotationCanvas))
        {
            DependencyObject? parent = current is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
            if (ReferenceEquals(parent, AnnotationCanvas))
            {
                return current as UIElement;
            }

            current = parent;
        }

        return null;
    }

    private static void TranslateAnnotations(IEnumerable<UIElement> elements, Vector delta)
    {
        foreach (UIElement element in elements)
        {
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);
            Canvas.SetLeft(element, (double.IsNaN(left) ? 0 : left) + delta.X);
            Canvas.SetTop(element, (double.IsNaN(top) ? 0 : top) + delta.Y);
        }
    }

    private void BeginErasing()
    {
        _erasing = true;
        _eraserOriginalClips.Clear();
        AnnotationCanvas.CaptureMouse();
        UpdateEraserPreview(_startPoint);
        EraseBetween(_startPoint, _startPoint);
    }

    private void UpdateEraserPreview(Point point)
    {
        if (_eraserPreview is not null)
        {
            SelectionCanvas.Children.Remove(_eraserPreview);
        }

        double size = EraserSizeSlider.Value;
        _eraserPreview = _eraserMode == EraserMode.Circle
            ? new Ellipse()
            : new Rectangle();
        _eraserPreview.Width = size;
        _eraserPreview.Height = size;
        _eraserPreview.Stroke = (Brush)FindResource("FirawCyan");
        _eraserPreview.StrokeThickness = 2;
        _eraserPreview.Fill = new SolidColorBrush(Color.FromArgb(35, 25, 211, 230));
        Canvas.SetLeft(_eraserPreview, point.X - (size / 2));
        Canvas.SetTop(_eraserPreview, point.Y - (size / 2));
        SelectionCanvas.Children.Add(_eraserPreview);
    }

    private void EraseBetween(Point from, Point to)
    {
        double size = EraserSizeSlider.Value;
        Geometry stroke;
        if ((to - from).Length < 0.1)
        {
            stroke = _eraserMode == EraserMode.Circle
                ? new EllipseGeometry(from, size / 2, size / 2)
                : new RectangleGeometry(new Rect(from.X - size / 2, from.Y - size / 2, size, size));
        }
        else
        {
            System.Windows.Media.Pen pen = new(Brushes.Black, size)
            {
                StartLineCap = _eraserMode == EraserMode.Circle ? PenLineCap.Round : PenLineCap.Square,
                EndLineCap = _eraserMode == EraserMode.Circle ? PenLineCap.Round : PenLineCap.Square,
                LineJoin = _eraserMode == EraserMode.Circle ? PenLineJoin.Round : PenLineJoin.Bevel
            };
            stroke = new LineGeometry(from, to).GetWidenedPathGeometry(pen);
        }

        foreach (UIElement annotation in AnnotationCanvas.Children.OfType<UIElement>().ToArray())
        {
            if (!GetAnnotationBounds(annotation).IntersectsWith(stroke.Bounds))
            {
                continue;
            }

            GeneralTransform? inverse = annotation.TransformToAncestor(AnnotationCanvas).Inverse;
            if (inverse is null ||
                !inverse.TryTransform(new Point(0, 0), out Point origin) ||
                !inverse.TryTransform(new Point(1, 0), out Point xAxis) ||
                !inverse.TryTransform(new Point(0, 1), out Point yAxis))
            {
                continue;
            }

            Rect localBounds = VisualTreeHelper.GetDescendantBounds(annotation);
            if ((localBounds.IsEmpty || localBounds.Width <= 0 || localBounds.Height <= 0) &&
                annotation is FrameworkElement frameworkElement)
            {
                localBounds = new Rect(0, 0,
                    Math.Max(0, frameworkElement.ActualWidth),
                    Math.Max(0, frameworkElement.ActualHeight));
            }

            if (localBounds.IsEmpty)
            {
                continue;
            }

            _eraserOriginalClips.TryAdd(annotation, annotation.Clip);
            Geometry localStroke = stroke.Clone();
            localStroke.Transform = new MatrixTransform(new Matrix(
                xAxis.X - origin.X, xAxis.Y - origin.Y,
                yAxis.X - origin.X, yAxis.Y - origin.Y,
                origin.X, origin.Y));
            annotation.Clip = new CombinedGeometry(GeometryCombineMode.Exclude,
                annotation.Clip ?? new RectangleGeometry(localBounds), localStroke);
        }
    }

    private void CompleteErasing()
    {
        _erasing = false;
        AnnotationCanvas.ReleaseMouseCapture();
        if (_eraserPreview is not null)
        {
            SelectionCanvas.Children.Remove(_eraserPreview);
            _eraserPreview = null;
        }

        if (_eraserOriginalClips.Count == 0)
        {
            EditorStatus.Text = "Nada encontrado no caminho da borracha.";
            return;
        }

        ClippedAnnotation[] clipped = [.. _eraserOriginalClips.Select(item =>
            new ClippedAnnotation(item.Key, item.Value, item.Key.Clip))];
        _history.Add(new EditorAction(
            () => ApplyClips(clipped, restoreOriginal: true),
            () => ApplyClips(clipped, restoreOriginal: false)));
        _eraserOriginalClips.Clear();
        EditorStatus.Text = "Área percorrida apagada. Ctrl+Z para desfazer.";
        UpdateHistoryButtons();
    }

    private static void ApplyClips(IEnumerable<ClippedAnnotation> annotations, bool restoreOriginal)
    {
        foreach (ClippedAnnotation annotation in annotations)
        {
            annotation.Element.Clip = restoreOriginal ? annotation.Before : annotation.After;
        }
    }

    private void RestoreAnnotations(IEnumerable<RemovedAnnotation> annotations)
    {
        foreach (RemovedAnnotation annotation in annotations.OrderBy(item => item.Index))
        {
            if (!AnnotationCanvas.Children.Contains(annotation.Element))
            {
                AnnotationCanvas.Children.Insert(
                    Math.Min(annotation.Index, AnnotationCanvas.Children.Count),
                    annotation.Element);
            }
        }
    }

    private void RemoveAnnotations(IEnumerable<UIElement> annotations)
    {
        foreach (UIElement annotation in annotations)
        {
            AnnotationCanvas.Children.Remove(annotation);
            _selectedElements.Remove(annotation);
        }

        UpdateSelectionOutline();
    }

    private void DeleteSelectedAnnotations()
    {
        RemovedAnnotation[] removed = [.. _selectedElements
            .Where(AnnotationCanvas.Children.Contains)
            .Select(element => new RemovedAnnotation(element, AnnotationCanvas.Children.IndexOf(element)))
            .OrderBy(item => item.Index)];
        if (removed.Length == 0)
        {
            return;
        }

        RemoveAnnotations(removed.Select(item => item.Element));
        _history.Add(new EditorAction(
            () => RestoreAnnotations(removed),
            () => RemoveAnnotations(removed.Select(item => item.Element))));
        EditorStatus.Text = $"{removed.Length} objeto(s) excluído(s). Ctrl+Z para desfazer.";
        UpdateHistoryButtons();
    }

    private void BeginTextRegionSelection()
    {
        _selectingTextRegion = true;
        AnnotationCanvas.CaptureMouse();
        _textSelectionBox = new Rectangle
        {
            Stroke = (Brush)FindResource("FirawCyan"),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 3 },
            Fill = new SolidColorBrush(Color.FromArgb(28, 25, 211, 230))
        };
        AnnotationCanvas.Children.Add(_textSelectionBox);
        UpdateRectangle(_textSelectionBox, _startPoint, _startPoint);
    }

    private async Task CompleteTextRegionSelectionAsync(Point end)
    {
        _selectingTextRegion = false;
        AnnotationCanvas.ReleaseMouseCapture();

        if (_textSelectionBox is not null)
        {
            UpdateRectangle(_textSelectionBox, _startPoint, end);
            AnnotationCanvas.Children.Remove(_textSelectionBox);
            _textSelectionBox = null;
        }

        _textSelectionMode = false;
        SelectTextButton.IsChecked = false;
        Rect region = CreateRect(_startPoint, end);
        if (region.Width < 4 || region.Height < 4)
        {
            EditorStatus.Text = "A região de texto ficou pequena demais. Tente novamente.";
            return;
        }

        BitmapSource composition = RenderCaptureComposition();
        double scaleX = composition.PixelWidth / AnnotationCanvas.ActualWidth;
        double scaleY = composition.PixelHeight / AnnotationCanvas.ActualHeight;
        Int32Rect pixelRegion = CaptureService.NormalizeSelection(
            region.Left * scaleX,
            region.Top * scaleY,
            region.Right * scaleX,
            region.Bottom * scaleY,
            new Int32Rect(0, 0, composition.PixelWidth, composition.PixelHeight));
        BitmapSource selectedRegion = _captureService.Crop(composition, pixelRegion);
        await RecognizeAndCopyTextAsync(selectedRegion, isSelectedRegion: true);
    }

    private static void UpdateRectangle(Rectangle rectangle, Point start, Point end)
    {
        Rect region = CreateRect(start, end);
        Canvas.SetLeft(rectangle, region.Left);
        Canvas.SetTop(rectangle, region.Top);
        rectangle.Width = region.Width;
        rectangle.Height = region.Height;
    }

    private void CancelTextSelection()
    {
        if (_selectingTextRegion)
        {
            AnnotationCanvas.ReleaseMouseCapture();
        }

        if (_textSelectionBox is not null)
        {
            AnnotationCanvas.Children.Remove(_textSelectionBox);
            _textSelectionBox = null;
        }

        _selectingTextRegion = false;
        _textSelectionMode = false;
        if (SelectTextButton is not null)
        {
            SelectTextButton.IsChecked = false;
        }
    }

    private UIElement CreateDraft(Point start)
    {
        Brush brush = CurrentBrush();
        double thickness = ThicknessSlider.Value;

        return _currentTool switch
        {
            EditorTool.Pencil => CreatePolyline(start, brush, thickness, 1),
            EditorTool.Highlight => CreatePolyline(start, brush, Math.Max(12, thickness * 3), 0.42),
            EditorTool.Arrow => new Path
            {
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = CreateArrowGeometry(start, start, thickness)
            },
            EditorTool.Redact => new Rectangle { Fill = Brushes.Black },
            EditorTool.Blur => new Rectangle
            {
                Stroke = (Brush)FindResource("FirawCyan"),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                Fill = new SolidColorBrush(Color.FromArgb(60, 25, 211, 230))
            },
            _ => new Rectangle
            {
                Stroke = brush,
                StrokeThickness = thickness,
                Fill = Brushes.Transparent
            }
        };
    }

    private static Polyline CreatePolyline(Point start, Brush brush, double thickness, double opacity)
    {
        var line = new Polyline
        {
            Stroke = brush,
            StrokeThickness = thickness,
            StrokeLineJoin = PenLineJoin.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Opacity = opacity
        };
        line.Points.Add(start);
        return line;
    }

    private void UpdateDraft(UIElement element, Point start, Point end)
    {
        if (element is Polyline line)
        {
            line.Points.Add(end);
            return;
        }

        if (element is Path arrow)
        {
            arrow.Data = CreateArrowGeometry(start, end, arrow.StrokeThickness);
            return;
        }

        if (element is Rectangle rectangle)
        {
            double left = Math.Min(start.X, end.X);
            double top = Math.Min(start.Y, end.Y);
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            rectangle.Width = Math.Abs(end.X - start.X);
            rectangle.Height = Math.Abs(end.Y - start.Y);
        }
    }

    private UIElement CreateBlurAnnotation(Rect displayRegion, BitmapSource? composition = null)
    {
        composition ??= RenderCaptureComposition();
        double scaleX = composition.PixelWidth / AnnotationCanvas.ActualWidth;
        double scaleY = composition.PixelHeight / AnnotationCanvas.ActualHeight;
        Int32Rect pixelRegion = CaptureService.NormalizeSelection(
            displayRegion.Left * scaleX,
            displayRegion.Top * scaleY,
            displayRegion.Right * scaleX,
            displayRegion.Bottom * scaleY,
            new Int32Rect(0, 0, composition.PixelWidth, composition.PixelHeight));
        BitmapSource crop = _captureService.Crop(composition, pixelRegion);

        var blurredImage = new Image
        {
            Source = crop,
            Width = displayRegion.Width,
            Height = displayRegion.Height,
            Stretch = Stretch.Fill,
            Effect = new BlurEffect
            {
                Radius = 18,
                KernelType = KernelType.Gaussian,
                RenderingBias = RenderingBias.Quality
            }
        };
        var container = new Grid
        {
            Width = displayRegion.Width,
            Height = displayRegion.Height,
            ClipToBounds = true,
            Background = Brushes.Transparent
        };
        container.Children.Add(blurredImage);
        Canvas.SetLeft(container, displayRegion.Left);
        Canvas.SetTop(container, displayRegion.Top);
        return container;
    }

    private async void AutoBlurButton_Click(object sender, RoutedEventArgs e)
    {
        AutoBlurButton.IsEnabled = false;
        EditorStatus.Text = "Procurando dados sensíveis localmente…";

        try
        {
            BitmapSource composition = RenderCaptureComposition();
            IReadOnlyList<OcrTextRegion> regions = await _ocrService.RecognizeTextRegionsAsync(composition);
            OcrTextRegion[] sensitiveRegions = regions
                .Where(region => _sensitiveDataDetector.IsSensitive(region.Text))
                .ToArray();
            if (sensitiveRegions.Length == 0)
            {
                EditorStatus.Text = "Nenhum e-mail, telefone, documento, cartão ou IP foi identificado.";
                return;
            }

            double scaleX = AnnotationCanvas.ActualWidth / composition.PixelWidth;
            double scaleY = AnnotationCanvas.ActualHeight / composition.PixelHeight;
            var annotations = new List<UIElement>();
            foreach (OcrTextRegion region in sensitiveRegions)
            {
                Rect displayRegion = new(
                    Math.Max(0, region.Bounds.X * scaleX - 5),
                    Math.Max(0, region.Bounds.Y * scaleY - 4),
                    Math.Min(AnnotationCanvas.ActualWidth - region.Bounds.X * scaleX + 5, region.Bounds.Width * scaleX + 10),
                    Math.Min(AnnotationCanvas.ActualHeight - region.Bounds.Y * scaleY + 4, region.Bounds.Height * scaleY + 8));
                if (displayRegion.Width < 2 || displayRegion.Height < 2)
                {
                    continue;
                }

                UIElement blurred = CreateBlurAnnotation(displayRegion, composition);
                AnnotationCanvas.Children.Add(blurred);
                annotations.Add(blurred);
            }

            if (annotations.Count > 0)
            {
                RecordAddedAnnotations(annotations);
                EditorStatus.Text = $"{annotations.Count} área(s) sensível(is) borrada(s). Revise antes de compartilhar.";
            }
            else
            {
                EditorStatus.Text = "Os dados encontrados estavam fora da área válida da imagem.";
            }
        }
        catch (Exception exception)
        {
            EditorStatus.Text = "Não foi possível analisar os dados sensíveis.";
            MessageBox.Show(this, exception.Message, "Firaw - Proteção de dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            AutoBlurButton.IsEnabled = true;
        }
    }

    private static Geometry CreateArrowGeometry(Point start, Point end, double thickness)
    {
        var geometry = new GeometryGroup();
        geometry.Children.Add(new LineGeometry(start, end));

        Vector direction = start - end;
        if (direction.Length < 0.5)
        {
            return geometry;
        }

        direction.Normalize();
        double headLength = Math.Max(12, thickness * 4);
        Vector normal = new(-direction.Y, direction.X);
        Point wingOne = end + (direction * headLength) + (normal * headLength * 0.48);
        Point wingTwo = end + (direction * headLength) - (normal * headLength * 0.48);
        geometry.Children.Add(new LineGeometry(end, wingOne));
        geometry.Children.Add(new LineGeometry(end, wingTwo));
        return geometry;
    }

    private void AddText(Point position, bool asNote)
    {
        var prompt = new TextPromptWindow(asNote ? "Escreva a anotação" : "Digite o texto")
        {
            Owner = this
        };
        if (prompt.ShowDialog() != true || string.IsNullOrWhiteSpace(prompt.ResultText))
        {
            return;
        }

        var text = new TextBlock
        {
            Text = prompt.ResultText,
            Foreground = CurrentBrush(),
            FontSize = Math.Max(18, ThicknessSlider.Value * 4),
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = Math.Max(120, AnnotationCanvas.ActualWidth - position.X)
        };

        UIElement annotation = text;
        if (asNote)
        {
            double availableWidth = Math.Max(160, AnnotationCanvas.ActualWidth - position.X);
            annotation = new Border
            {
                Width = Math.Min(320, availableWidth),
                MinHeight = 76,
                Padding = new Thickness(13, 11, 13, 12),
                CornerRadius = new CornerRadius(9),
                Background = new SolidColorBrush(Color.FromArgb(232, 21, 29, 38)),
                BorderBrush = CurrentBrush(),
                BorderThickness = new Thickness(2),
                Child = text
            };
        }

        Canvas.SetLeft(annotation, position.X);
        Canvas.SetTop(annotation, position.Y);
        AnnotationCanvas.Children.Add(annotation);
        RecordAddedAnnotations([annotation]);
        EditorStatus.Text = asNote
            ? "Anotação criada. Use Selecionar para movê-la."
            : "Texto criado. Use Selecionar para movê-lo.";
    }

    private void RecordAddedAnnotations(IEnumerable<UIElement> annotations)
    {
        UIElement[] added = [.. annotations];
        _history.Add(new EditorAction(
            () => RemoveAnnotations(added),
            () =>
            {
                foreach (UIElement annotation in added)
                {
                    if (!AnnotationCanvas.Children.Contains(annotation))
                    {
                        AnnotationCanvas.Children.Add(annotation);
                    }
                }
            }));
        UpdateHistoryButtons();
    }

    private void EraserModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EraserModeCombo?.SelectedItem is ComboBoxItem item &&
            Enum.TryParse(item.Tag?.ToString(), out EraserMode mode))
        {
            _eraserMode = mode;
            if (_currentTool == EditorTool.Eraser)
            {
                EditorStatus.Text = $"Borracha: {item.Content}.";
            }
        }
    }

    private Brush CurrentBrush()
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(_currentColor)!;
        brush.Freeze();
        return brush;
    }

    private void ColorSwatch_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton selected || ColorPalettePanel is null)
        {
            return;
        }

        _currentColor = selected.Tag?.ToString() ?? "#19D3E6";
        foreach (ToggleButton swatch in ColorPalettePanel.Children.OfType<ToggleButton>())
        {
            if (!ReferenceEquals(swatch, selected))
            {
                swatch.IsChecked = false;
            }
        }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSelection();
        EditorAction? action = _history.Undo();
        if (action is not null)
        {
            action.Undo();
            EditorStatus.Text = "Ação desfeita.";
        }
        UpdateHistoryButtons();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSelection();
        EditorAction? action = _history.Redo();
        if (action is not null)
        {
            action.Redo();
            EditorStatus.Text = "Ação refeita.";
        }
        UpdateHistoryButtons();
    }

    private void CopyImageButton_Click(object sender, RoutedEventArgs e)
    {
        BitmapSource rendered = RenderEditedImage();
        if (TryClipboard(() => System.Windows.Clipboard.SetImage(rendered)))
        {
            EditorStatus.Text = "Imagem copiada para a área de transferência.";
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Salvar captura",
            Filter = "Imagem PNG (*.png)|*.png",
            DefaultExt = ".png",
            AddExtension = true,
            FileName = $"Firaw-{DateTime.Now:yyyy-MM-dd-HHmmss}.png"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(RenderEditedImage()));
        using FileStream stream = File.Create(dialog.FileName);
        encoder.Save(stream);
        EditorStatus.Text = $"Salvo: {System.IO.Path.GetFileName(dialog.FileName)}";
    }

    private async void CopyTextButton_Click(object sender, RoutedEventArgs e)
    {
        CancelTextSelection();
        await RecognizeAndCopyTextAsync(RenderCaptureComposition(), isSelectedRegion: false);
    }

    private void SelectTextButton_Click(object sender, RoutedEventArgs e)
    {
        bool enable = SelectTextButton.IsChecked == true;
        CancelTextSelection();
        if (!enable)
        {
            EditorStatus.Text = "Seleção de texto cancelada.";
            return;
        }

        SelectTool.IsChecked = true;
        _textSelectionMode = true;
        SelectTextButton.IsChecked = true;
        EditorStatus.Text = "Arraste uma moldura sobre o texto que deseja copiar.";
    }

    private async Task RecognizeAndCopyTextAsync(BitmapSource source, bool isSelectedRegion)
    {
        CopyTextButton.IsEnabled = false;
        SelectTextButton.IsEnabled = false;
        EditorStatus.Text = isSelectedRegion
            ? "Reconhecendo o trecho selecionado…"
            : "Reconhecendo todo o texto localmente…";

        try
        {
            string text = await _ocrService.RecognizeAsync(source);
            if (string.IsNullOrWhiteSpace(text))
            {
                EditorStatus.Text = "Nenhum texto foi encontrado nessa captura.";
                return;
            }

            string textToCopy = text.Trim();
            if (TryClipboard(() => System.Windows.Clipboard.SetText(textToCopy)))
            {
                _textHistory.Add(
                    textToCopy,
                    isSelectedRegion ? "OCR selecionado" : "OCR completo");
                EditorStatus.Text = isSelectedRegion
                    ? "Trecho selecionado copiado para a área de transferência."
                    : "Todo o texto reconhecido foi copiado.";
            }
        }
        catch (Exception exception)
        {
            EditorStatus.Text = "Não foi possível reconhecer o texto.";
            MessageBox.Show(this, exception.Message, "Firaw - OCR", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            CopyTextButton.IsEnabled = true;
            SelectTextButton.IsEnabled = true;
        }
    }

    private BitmapSource RenderCaptureComposition()
    {
        (UIElement Element, Visibility Visibility)[] annotations = AnnotationCanvas.Children
            .OfType<UIElement>()
            .Where(element => !IsCaptureAnnotation(element))
            .Select(element => (element, element.Visibility))
            .ToArray();
        try
        {
            foreach ((UIElement element, _) in annotations)
            {
                element.Visibility = Visibility.Collapsed;
            }
            return RenderEditedImage();
        }
        finally
        {
            foreach ((UIElement element, Visibility visibility) in annotations)
            {
                element.Visibility = visibility;
            }
        }
    }

    private BitmapSource RenderEditedImage()
    {
        Visibility previousVisibility = SelectionCanvas.Visibility;
        SelectionCanvas.Visibility = Visibility.Collapsed;
        try
        {
            EditorSurface.UpdateLayout();
            int width = Math.Max(1, (int)Math.Ceiling(EditorSurface.Width));
            int height = Math.Max(1, (int)Math.Ceiling(EditorSurface.Height));
            var result = new RenderTargetBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Pbgra32);
            result.Render(EditorSurface);
            result.Freeze();
            return result;
        }
        finally
        {
            SelectionCanvas.Visibility = previousVisibility;
        }
    }

    private bool TryClipboard(Action write)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                write();
                return true;
            }
            catch (System.Runtime.InteropServices.COMException) when (attempt < 2)
            {
                Thread.Sleep(40);
            }
        }

        EditorStatus.Text = "Não foi possível acessar a área de transferência. Tente novamente.";
        return false;
    }

    private void TextHistoryToggle_Click(object sender, RoutedEventArgs e)
    {
        TextHistoryDrawer.Visibility = TextHistoryToggle.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void CloseTextHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        TextHistoryToggle.IsChecked = false;
        TextHistoryDrawer.Visibility = Visibility.Collapsed;
    }

    private void ImportClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                EditorStatus.Text = "A área de transferência não contém texto.";
                return;
            }

            string text = System.Windows.Clipboard.GetText(TextDataFormat.UnicodeText);
            _textHistory.Add(text, "Importado manualmente");
            EditorStatus.Text = "Texto atual incluído na lista.";
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            EditorStatus.Text = "A área de transferência está ocupada. Tente novamente.";
        }
    }

    private void CopySelectedHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        CopiedTextEntry[] selected = CopiedTextList.SelectedItems
            .OfType<CopiedTextEntry>()
            .ToArray();
        if (selected.Length == 0)
        {
            EditorStatus.Text = "Selecione pelo menos um texto na lista.";
            return;
        }

        string combined = TextHistoryService.Combine(selected);
        if (TryClipboard(() => System.Windows.Clipboard.SetText(combined)))
        {
            _textHistory.Add(combined, $"{selected.Length} textos combinados");
            EditorStatus.Text = $"{selected.Length} texto(s) copiado(s) em conjunto.";
        }
    }

    private void ClearTextHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        _textHistory.Items.Clear();
        EditorStatus.Text = "Lista de textos limpa.";
    }

    private void TextHistory_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        UpdateTextHistoryUi();

    private void UpdateTextHistoryUi()
    {
        int count = _textHistory.Items.Count;
        TextHistoryToggle.Content = $"Textos ({count})";
        EmptyHistoryText.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateHistoryButtons()
    {
        UndoButton.IsEnabled = _history.CanUndo;
        RedoButton.IsEnabled = _history.CanRedo;
    }

    private Point ClampToCanvas(Point point) => new(
        Math.Clamp(point.X, 0, AnnotationCanvas.ActualWidth),
        Math.Clamp(point.Y, 0, AnnotationCanvas.ActualHeight));

    private static Rect CreateRect(Point first, Point second) => new(
        new Point(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y)),
        new Point(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y)));

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
        {
            CopyImageButton_Click(sender, e);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
        {
            UndoButton_Click(sender, e);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
        {
            RedoButton_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && (_textSelectionMode || _selectingTextRegion))
        {
            CancelTextSelection();
            EditorStatus.Text = "Seleção de texto cancelada.";
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _selectedElements.Count > 0)
        {
            ClearSelection();
            EditorStatus.Text = "Seleção de objetos cancelada.";
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None &&
                 _currentTool == EditorTool.Select &&
                 Keyboard.FocusedElement is not System.Windows.Controls.Primitives.TextBoxBase)
        {
            DeleteSelectedAnnotations();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.None && TrySelectToolByNumber(e.Key))
        {
            e.Handled = true;
        }
    }

    private bool TrySelectToolByNumber(Key key)
    {
        int index = key switch
        {
            Key.D1 or Key.NumPad1 => 0,
            Key.D2 or Key.NumPad2 => 1,
            Key.D3 or Key.NumPad3 => 2,
            Key.D4 or Key.NumPad4 => 3,
            Key.D5 or Key.NumPad5 => 4,
            Key.D6 or Key.NumPad6 => 5,
            Key.D7 or Key.NumPad7 => 6,
            Key.D8 or Key.NumPad8 => 7,
            Key.D9 or Key.NumPad9 => 8,
            Key.D0 or Key.NumPad0 => 9,
            _ => -1
        };
        ToggleButton[] tools = ToolsPanel.Children.OfType<ToggleButton>().ToArray();
        if (index < 0 || index >= tools.Length)
        {
            return false;
        }

        tools[index].IsChecked = true;
        return true;
    }

    private sealed record RemovedAnnotation(UIElement Element, int Index);
    private sealed record ClippedAnnotation(UIElement Element, Geometry? Before, Geometry? After);
}
