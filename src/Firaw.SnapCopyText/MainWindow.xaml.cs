using System.Windows;
using Firaw.SnapCopyText.Services;
using Firaw.SnapCopyText.Views;
using MessageBox = System.Windows.MessageBox;

namespace Firaw.SnapCopyText;

public partial class MainWindow : Window
{
    private readonly CaptureService _captureService = new();
    private readonly CaptureRequestGate _captureGate = new();
    private HotkeyService? _hotkeyService;

    public event EventHandler? CaptureRequested;

    public MainWindow()
    {
        InitializeComponent();
        CaptureRequested += async (_, _) => await BeginCaptureAsync();
        SourceInitialized += MainWindow_SourceInitialized;
        Closed += (_, _) => _hotkeyService?.Dispose();
    }

    public void SetStatus(string message) => StatusText.Text = message;

    private void CaptureButton_Click(object sender, RoutedEventArgs e) =>
        CaptureRequested?.Invoke(this, EventArgs.Empty);

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _hotkeyService = new HotkeyService(this);
        _hotkeyService.CaptureRequested += (_, _) => CaptureRequested?.Invoke(this, EventArgs.Empty);
        _hotkeyService.Initialize();

        SetStatus((_hotkeyService.PrintScreenRegistered, _hotkeyService.FallbackRegistered) switch
        {
            (true, true) => "Pronto • Print Screen ou Ctrl + Shift + S",
            (true, false) => "Pronto • Print Screen",
            (false, true) => "Print Screen está ocupado • use Ctrl + Shift + S",
            _ => "Atalhos ocupados • use o botão Nova captura"
        });
    }

    private async Task BeginCaptureAsync()
    {
        if (!_captureGate.TryEnter())
        {
            return;
        }

        try
        {
            Hide();
            await Task.Delay(120);

            DesktopSnapshot snapshot = _captureService.CaptureVirtualScreen();
            var overlay = new CaptureOverlayWindow(snapshot, _captureService);
            bool selected = overlay.ShowDialog() == true && overlay.SelectedImage is not null;

            Show();
            Activate();

            if (selected)
            {
                var editor = new EditorWindow(overlay.SelectedImage!) { Owner = this };
                editor.Show();
                SetStatus("Captura aberta no editor.");
            }
            else
            {
                SetStatus("Captura cancelada.");
            }
        }
        catch (Exception exception)
        {
            Show();
            Activate();
            SetStatus("Não foi possível iniciar a captura.");
            MessageBox.Show(this, exception.Message, "Firaw - Captura", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _captureGate.Exit();
        }
    }
}
