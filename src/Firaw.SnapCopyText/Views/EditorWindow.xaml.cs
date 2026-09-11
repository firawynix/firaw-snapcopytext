using System.IO;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.MessageBox;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Firaw.SnapCopyText.Views;

public partial class EditorWindow : Window
{
    private readonly AnnotationHistory<UIElement> _history = new();
    private readonly OcrService _ocrService = new();
    private EditorTool _currentTool = EditorTool.Select;
    private Point _startPoint;
    private UIElement? _draft;
    private bool _drawing;

    public BitmapSource OriginalImage { get; }

    public EditorWindow(BitmapSource image)
    {
        InitializeComponent();
        OriginalImage = image;
        CaptureImage.Source = image;
        EditorSurface.Width = image.PixelWidth;
        EditorSurface.Height = image.PixelHeight;
    }

    private void ToolButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton selected || ToolsPanel is null)
        {
            return;
        }

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
        _startPoint = e.GetPosition(AnnotationCanvas);

        if (_currentTool == EditorTool.Select)
        {
            return;
        }

        if (_currentTool == EditorTool.Text)
        {
            AddText(_startPoint);
            return;
        }

        _drawing = true;
        AnnotationCanvas.CaptureMouse();
        _draft = CreateDraft(_startPoint);
        AnnotationCanvas.Children.Add(_draft);
    }

    private void AnnotationCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_drawing || _draft is null)
        {
            return;
        }

        Point current = ClampToCanvas(e.GetPosition(AnnotationCanvas));
        UpdateDraft(_draft, _startPoint, current);
    }

    private void AnnotationCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
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

    private void AddText(Point position)
    {
        var prompt = new TextPromptWindow { Owner = this };
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
        Canvas.SetLeft(text, position.X);
        Canvas.SetTop(text, position.Y);
        AnnotationCanvas.Children.Add(text);
        _history.Add(text);
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
        CopyTextButton.IsEnabled = false;
        EditorStatus.Text = "Reconhecendo texto localmente…";

        try
        {
            string text = await _ocrService.RecognizeAsync(OriginalImage);
            if (string.IsNullOrWhiteSpace(text))
            {
                EditorStatus.Text = "Nenhum texto foi encontrado nessa captura.";
                return;
            }

            var preview = new OcrPreviewWindow(text) { Owner = this };
            if (preview.ShowDialog() == true && !string.IsNullOrWhiteSpace(preview.ResultText) &&
                TryClipboard(() => System.Windows.Clipboard.SetText(preview.ResultText)))
            {
                EditorStatus.Text = "Texto copiado para a área de transferência.";
            }
            else
            {
                EditorStatus.Text = "Cópia de texto cancelada.";
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

    private void UpdateHistoryButtons()
    {
        UndoButton.IsEnabled = _history.CanUndo;
        RedoButton.IsEnabled = _history.CanRedo;
    }

    private Point ClampToCanvas(Point point) => new(
        Math.Clamp(point.X, 0, AnnotationCanvas.ActualWidth),
        Math.Clamp(point.Y, 0, AnnotationCanvas.ActualHeight));

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
    }
}
