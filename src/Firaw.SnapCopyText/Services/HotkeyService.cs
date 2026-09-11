using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Firaw.SnapCopyText.Services;

public sealed class HotkeyService : IDisposable
{
    private const int PrintScreenId = 0x5343;
    private const int FallbackId = 0x5344;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkSnapshot = 0x2C;

    private readonly Window _window;
    private nint _windowHandle;
    private HwndSource? _source;
    private bool _disposed;

    public bool PrintScreenRegistered { get; private set; }
    public bool FallbackRegistered { get; private set; }
    public event EventHandler? CaptureRequested;

    public HotkeyService(Window window)
    {
        _window = window;
    }

    public void Initialize()
    {
        if (_windowHandle != nint.Zero)
        {
            return;
        }

        _windowHandle = new WindowInteropHelper(_window).Handle;
        if (_windowHandle == nint.Zero)
        {
            throw new InvalidOperationException("A janela precisa estar inicializada antes de registrar atalhos.");
        }

        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(WindowProcedure);

        PrintScreenRegistered = RegisterHotKey(_windowHandle, PrintScreenId, ModNoRepeat, VkSnapshot);
        uint sKey = (uint)KeyInterop.VirtualKeyFromKey(Key.S);
        FallbackRegistered = RegisterHotKey(
            _windowHandle,
            FallbackId,
            ModControl | ModShift | ModNoRepeat,
            sKey);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_windowHandle != nint.Zero)
        {
            if (PrintScreenRegistered)
            {
                UnregisterHotKey(_windowHandle, PrintScreenId);
            }
            if (FallbackRegistered)
            {
                UnregisterHotKey(_windowHandle, FallbackId);
            }
        }

        _source?.RemoveHook(WindowProcedure);
    }

    private nint WindowProcedure(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == WmHotkey && (wParam.ToInt32() == PrintScreenId || wParam.ToInt32() == FallbackId))
        {
            handled = true;
            CaptureRequested?.Invoke(this, EventArgs.Empty);
        }

        return nint.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);
}
