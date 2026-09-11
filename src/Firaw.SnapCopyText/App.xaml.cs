using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Resources;
using Firaw.SnapCopyText.Services;
using Application = System.Windows.Application;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

namespace Firaw.SnapCopyText;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private NotifyIcon? _trayIcon;
    private bool _isExiting;
    private bool _backgroundHintShown;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(Window_Loaded));

        _mainWindow = new MainWindow();
        MainWindow = _mainWindow;
        _mainWindow.Closing += MainWindow_Closing;
        _mainWindow.StateChanged += MainWindow_StateChanged;

        _trayIcon = new NotifyIcon
        {
            Icon = LoadApplicationIcon(),
            Text = "Firaw - SnapCopyText",
            Visible = true,
            ContextMenuStrip = CreateTrayMenu()
        };
        _trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowMainWindow);

        _mainWindow.Show();
    }

    private static void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Window window)
        {
            WindowBrandingService.Apply(window);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        base.OnExit(e);
    }

    private ContextMenuStrip CreateTrayMenu()
    {
        var menu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Abrir Firaw");
        openItem.Click += (_, _) => Dispatcher.Invoke(ShowMainWindow);
        menu.Items.Add(openItem);

        var captureItem = new ToolStripMenuItem("Nova captura");
        captureItem.Click += (_, _) => Dispatcher.Invoke(() => _mainWindow?.RequestCapture());
        menu.Items.Add(captureItem);

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Sair");
        exitItem.Click += (_, _) => Dispatcher.Invoke(ExitApplication);
        menu.Items.Add(exitItem);

        return menu;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExiting || _mainWindow is null)
        {
            return;
        }

        e.Cancel = true;
        HideMainWindow();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_mainWindow?.WindowState == WindowState.Minimized)
        {
            HideMainWindow();
        }
    }

    private void HideMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Hide();
        _mainWindow.WindowState = WindowState.Normal;

        if (!_backgroundHintShown && _trayIcon is not null)
        {
            _backgroundHintShown = true;
            _trayIcon.ShowBalloonTip(
                2500,
                "Firaw continua ativo",
                "Use o Olho de Hórus na bandeja para abrir ou fazer uma nova captura.",
                System.Windows.Forms.ToolTipIcon.Info);
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        if (!_mainWindow.IsVisible)
        {
            _mainWindow.Show();
        }

        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        foreach (Window window in Windows.OfType<Window>().ToArray())
        {
            window.Close();
        }
        Shutdown();
    }

    private static Icon LoadApplicationIcon()
    {
        StreamResourceInfo resource = GetResourceStream(
            new Uri("pack://application:,,,/Assets/firaw-eye.ico", UriKind.Absolute));
        using Stream stream = resource.Stream;
        using var source = new Icon(stream);
        return (Icon)source.Clone();
    }
}
