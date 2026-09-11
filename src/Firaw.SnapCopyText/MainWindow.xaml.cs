using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Firaw.SnapCopyText.Services;
using Firaw.SnapCopyText.Views;
using Firaw.SnapCopyText.Models;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Firaw.SnapCopyText;

public partial class MainWindow : Window
{
    private const int DwmwaTransitionsForcedDisabled = 3;
    private const uint WdaExcludeFromCapture = 0x00000011;
    private readonly CaptureService _captureService = new();
    private readonly CaptureRequestGate _captureGate = new();
    private readonly WindowSelectionService _windowSelectionService = new();
    private readonly AppSettingsService _settingsService = new();
    private readonly StartupService _startupService = new();
    private CapturePreferences _preferences;
    private HotkeyService? _hotkeyService;
    private ClipboardMonitorService? _clipboardMonitorService;

    public MainWindow()
    {
        InitializeComponent();
        _preferences = _settingsService.Load();
        if (_preferences.StartWithWindows)
        {
            TryApplyStartupPreference(showError: false);
        }
        SourceInitialized += MainWindow_SourceInitialized;
        Closed += (_, _) =>
        {
            _hotkeyService?.Dispose();
            _clipboardMonitorService?.Dispose();
        };
    }

    public void SetStatus(string message) => StatusText.Text = message;

    public void RequestCapture() => RequestCapture(_preferences.DefaultMode);

    public void RequestCapture(CaptureMode mode) => _ = BeginCaptureAsync(mode);

    public void OpenSettings()
    {
        var settingsWindow = new SettingsWindow(_preferences);
        if (IsVisible)
        {
            settingsWindow.Owner = this;
        }

        if (settingsWindow.ShowDialog() != true || settingsWindow.SavedPreferences is null)
        {
            return;
        }

        _preferences = settingsWindow.SavedPreferences;
        _settingsService.Save(_preferences);
        TryApplyStartupPreference(showError: true);
        _hotkeyService?.Apply(_preferences);
        RefreshHotkeyStatus();
    }

    private void TryApplyStartupPreference(bool showError)
    {
        try
        {
            _startupService.SetEnabled(_preferences.StartWithWindows);
        }
        catch (Exception exception) when (!showError)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"Não foi possível alterar a inicialização com o Windows.\n\n{exception.Message}",
                "Firaw - Inicialização",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void CaptureModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element &&
            Enum.TryParse(element.Tag?.ToString(), out CaptureMode mode))
        {
            RequestCapture(mode);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) => OpenSettings();

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _hotkeyService = new HotkeyService(this);
        _hotkeyService.CaptureRequested += (_, _) => RequestCapture();
        _hotkeyService.Initialize(_preferences);
        _clipboardMonitorService = new ClipboardMonitorService(this, TextHistoryService.Shared);
        _clipboardMonitorService.Initialize();

        RefreshHotkeyStatus();
    }

    private void RefreshHotkeyStatus()
    {
        if (_hotkeyService is null)
        {
            return;
        }

        string modeLabel = ModeLabel(_preferences.DefaultMode);
        SetStatus((_hotkeyService.PrintScreenRegistered, _hotkeyService.FallbackRegistered, _preferences.UsePrintScreen) switch
        {
            (true, true, _) => $"Pronto • Print Screen ou {_hotkeyService.FallbackLabel} • padrão: {modeLabel}",
            (true, false, _) => $"Pronto • Print Screen • padrão: {modeLabel}",
            (false, true, true) => $"Print Screen está ocupado • use {_hotkeyService.FallbackLabel}",
            (false, true, false) => $"Pronto • {_hotkeyService.FallbackLabel} • padrão: {modeLabel}",
            (false, false, _) when _hotkeyService.FallbackLabel == "Print Screen" =>
                "Print Screen está ocupado • escolha outro atalho ou ajuste o Windows",
            _ => "Atalhos ocupados • use um dos botões de captura"
        });
    }

    private async Task BeginCaptureAsync(CaptureMode mode)
    {
        if (!_captureGate.TryEnter())
        {
            return;
        }

        List<HiddenWindowState> hiddenWindows = [];

        try
        {
            CaptureTarget? fixedTarget = mode == CaptureMode.Region ? null : ChooseCaptureTarget(mode);
            if (mode != CaptureMode.Region && fixedTarget is null)
            {
                SetStatus("Captura cancelada.");
                return;
            }

            hiddenWindows = HideVisibleFirawWindows();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            DwmFlush();
            await Task.Delay(260);

            DesktopSnapshot snapshot = _captureService.CaptureVirtualScreen();
            BitmapSource? selectedImage;
            bool selected;
            if (mode == CaptureMode.Region)
            {
                var overlay = new CaptureOverlayWindow(snapshot, _captureService);
                selected = overlay.ShowDialog() == true && overlay.SelectedImage is not null;
                selectedImage = overlay.SelectedImage;
            }
            else
            {
                selectedImage = mode == CaptureMode.Window
                    ? _captureService.CaptureWindow(fixedTarget!) ??
                      _captureService.CropScreenBounds(snapshot, fixedTarget!.Bounds)
                    : _captureService.CropScreenBounds(snapshot, fixedTarget!.Bounds);
                selected = true;
            }

            RestoreFirawWindows(hiddenWindows);
            hiddenWindows.Clear();

            if (selected)
            {
                var editor = new EditorWindow(selectedImage!);
                if (IsVisible)
                {
                    editor.Owner = this;
                }
                editor.Show();
                SetStatus($"Captura de {ModeLabel(mode).ToLowerInvariant()} aberta no editor.");
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

    private CaptureTarget? ChooseCaptureTarget(CaptureMode mode)
    {
        IReadOnlyList<CaptureTarget> targets = mode == CaptureMode.Monitor
            ? _windowSelectionService.GetMonitors()
            : _windowSelectionService.GetVisibleWindows();
        var picker = new CaptureTargetPickerWindow(mode, targets);
        if (IsVisible)
        {
            picker.Owner = this;
            picker.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        return picker.ShowDialog() == true ? picker.SelectedTarget : null;
    }

    private static string ModeLabel(CaptureMode mode) => mode switch
    {
        CaptureMode.Window => "Janela",
        CaptureMode.Monitor => "Monitor",
        _ => "Região"
    };

    private static List<HiddenWindowState> HideVisibleFirawWindows()
    {
        List<HiddenWindowState> states = Application.Current.Windows
            .OfType<Window>()
            .Where(window => window.IsVisible)
            .Select(window => new HiddenWindowState(
                window,
                window.WindowState,
                window.IsActive,
                AreTransitionsForcedDisabled(window),
                GetDisplayAffinity(window)))
            .ToList();

        foreach (HiddenWindowState state in states.OrderByDescending(state => state.Window.Owner is not null))
        {
            SetTransitionsForcedDisabled(state.Window, disabled: true);
            SetDisplayAffinity(state.Window, WdaExcludeFromCapture);
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
            SetDisplayAffinity(state.Window, state.DisplayAffinity);
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

    private static uint GetDisplayAffinity(Window window)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        return handle != nint.Zero && GetWindowDisplayAffinity(handle, out uint affinity)
            ? affinity
            : 0;
    }

    private static void SetDisplayAffinity(Window window, uint affinity)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        if (handle != nint.Zero)
        {
            _ = SetWindowDisplayAffinity(handle, affinity);
        }
    }

    private sealed record HiddenWindowState(
        Window Window,
        WindowState WindowState,
        bool WasActive,
        bool TransitionsWereDisabled,
        uint DisplayAffinity);

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowDisplayAffinity(nint windowHandle, out uint affinity);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(nint windowHandle, uint affinity);
}
