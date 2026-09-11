using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Firaw.SnapCopyText.Services;
using Firaw.SnapCopyText.Views;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Firaw.SnapCopyText;

public partial class MainWindow : Window
{
    private const int DwmwaTransitionsForcedDisabled = 3;
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

        List<HiddenWindowState> hiddenWindows = [];

        try
        {
            hiddenWindows = HideVisibleFirawWindows();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            DwmFlush();
            await Task.Delay(260);

            DesktopSnapshot snapshot = _captureService.CaptureVirtualScreen();
            var overlay = new CaptureOverlayWindow(snapshot, _captureService);
            bool selected = overlay.ShowDialog() == true && overlay.SelectedImage is not null;

            RestoreFirawWindows(hiddenWindows);
            hiddenWindows.Clear();

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
            RestoreFirawWindows(hiddenWindows);
            hiddenWindows.Clear();
            SetStatus("Não foi possível iniciar a captura.");
            MessageBox.Show(this, exception.Message, "Firaw - Captura", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            RestoreFirawWindows(hiddenWindows);
            _captureGate.Exit();
        }
    }

    private static List<HiddenWindowState> HideVisibleFirawWindows()
    {
        List<HiddenWindowState> states = Application.Current.Windows
            .OfType<Window>()
            .Where(window => window.IsVisible)
            .Select(window => new HiddenWindowState(
                window,
                window.WindowState,
                window.IsActive,
                AreTransitionsForcedDisabled(window)))
            .ToList();

        foreach (HiddenWindowState state in states.OrderByDescending(state => state.Window.Owner is not null))
        {
            SetTransitionsForcedDisabled(state.Window, disabled: true);
            state.Window.Hide();
        }

        return states;
    }

    private static void RestoreFirawWindows(IEnumerable<HiddenWindowState> states)
    {
        HiddenWindowState? activeWindow = null;

        foreach (HiddenWindowState state in states.OrderBy(state => state.Window.Owner is null ? 0 : 1))
        {
            state.Window.Show();
            state.Window.WindowState = state.WindowState;
            SetTransitionsForcedDisabled(state.Window, state.TransitionsWereDisabled);
            if (state.WasActive)
            {
                activeWindow = state;
            }
        }

        activeWindow?.Window.Activate();
    }

    private static bool AreTransitionsForcedDisabled(Window window)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        int disabled = 0;
        return handle != nint.Zero &&
               DwmGetWindowAttribute(
                   handle,
                   DwmwaTransitionsForcedDisabled,
                   out disabled,
                   Marshal.SizeOf<int>()) == 0 &&
               disabled != 0;
    }

    private static void SetTransitionsForcedDisabled(Window window, bool disabled)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        int value = disabled ? 1 : 0;
        _ = DwmSetWindowAttribute(
            handle,
            DwmwaTransitionsForcedDisabled,
            ref value,
            Marshal.SizeOf<int>());
    }

    private sealed record HiddenWindowState(
        Window Window,
        WindowState WindowState,
        bool WasActive,
        bool TransitionsWereDisabled);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        nint windowHandle,
        int attribute,
        out int attributeValue,
        int attributeSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        nint windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();
}
