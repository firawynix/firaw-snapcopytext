using System.IO;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.MessageBox;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextDataFormat = System.Windows.TextDataFormat;

namespace Firaw.SnapCopyText.Views;

public partial class EditorWindow : Window
{
    private readonly AnnotationHistory<UIElement> _history = new();
    private readonly OcrService _ocrService = new();
    private readonly CaptureService _captureService = new();
    private readonly TextHistoryService _textHistory = TextHistoryService.Shared;
    private EditorTool _currentTool = EditorTool.Select;
    private Point _startPoint;
    private UIElement? _draft;
    private Rectangle? _textSelectionBox;
    private bool _drawing;
    private bool _selectingTextRegion;
    private bool _textSelectionMode;

    public BitmapSource OriginalImage { get; }

    public EditorWindow(BitmapSource image)
    {
        InitializeComponent();
        OriginalImage = image;
        CaptureImage.Source = image;
        EditorSurface.Width = image.PixelWidth;
        EditorSurface.Height = image.PixelHeight;
        CopiedTextList.ItemsSource = _textHistory.Items;
        _textHistory.Items.CollectionChanged += TextHistory_CollectionChanged;
        Closed += (_, _) => _textHistory.Items.CollectionChanged -= TextHistory_CollectionChanged;
        UpdateTextHistoryUi();
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
        }
    }

    private void AnnotationCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = ClampToCanvas(e.GetPosition(AnnotationCanvas));

        if (_textSelectionMode)
        {
            BeginTextRegionSelection();
            return;
        }

        if (_currentTool == EditorTool.Select)
        {
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
        if (_selectingTextRegion && _textSelectionBox is not null)
        {
            UpdateRectangle(_textSelectionBox, _startPoint, ClampToCanvas(e.GetPosition(AnnotationCanvas)));
            return;
        }

        if (!_drawing || _draft is null)
        {
            return;
        }

        Point current = ClampToCanvas(e.GetPosition(AnnotationCanvas));
        UpdateDraft(_draft, _startPoint, current);
    }

    private async void AnnotationCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectingTextRegion)
        {
            await CompleteTextRegionSelectionAsync(ClampToCanvas(e.GetPosition(AnnotationCanvas)));
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
        else
        {
            _history.Add(_draft);
        }

        _draft = null;
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

        double scaleX = OriginalImage.PixelWidth / AnnotationCanvas.ActualWidth;
        double scaleY = OriginalImage.PixelHeight / AnnotationCanvas.ActualHeight;
        Int32Rect pixelRegion = CaptureService.NormalizeSelection(
            region.Left * scaleX,
            region.Top * scaleY,
            region.Right * scaleX,
            region.Bottom * scaleY,
            new Int32Rect(0, 0, OriginalImage.PixelWidth, OriginalImage.PixelHeight));
        BitmapSource selectedRegion = _captureService.Crop(OriginalImage, pixelRegion);
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
        _history.Add(annotation);
        UpdateHistoryButtons();
    }

    private Brush CurrentBrush()
    {
        string color = (ColorPicker.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "#19D3E6";
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
        brush.Freeze();
        return brush;
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        UIElement? item = _history.Undo();
        if (item is not null)
        {
            AnnotationCanvas.Children.Remove(item);
        }
        UpdateHistoryButtons();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        UIElement? item = _history.Redo();
        if (item is not null)
        {
            AnnotationCanvas.Children.Add(item);
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
        await RecognizeAndCopyTextAsync(OriginalImage, isSelectedRegion: false);
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

    private BitmapSource RenderEditedImage()
    {
        EditorSurface.UpdateLayout();
        var result = new RenderTargetBitmap(
            OriginalImage.PixelWidth,
            OriginalImage.PixelHeight,
            96,
            96,
            PixelFormats.Pbgra32);
        result.Render(EditorSurface);
        result.Freeze();
        return result;
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
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
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
    }
}
