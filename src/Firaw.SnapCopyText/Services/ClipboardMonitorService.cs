using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Clipboard = System.Windows.Clipboard;
using TextDataFormat = System.Windows.TextDataFormat;

namespace Firaw.SnapCopyText.Services;

public sealed class ClipboardMonitorService : IDisposable
{
    private const int WmClipboardUpdate = 0x031D;
    private readonly Window _window;
    private readonly TextHistoryService _history;
    private HwndSource? _source;
    private nint _windowHandle;

    public ClipboardMonitorService(Window window, TextHistoryService history)
    {
        _window = window;
        _history = history;
    }

    public bool Initialize()
    {
        _windowHandle = new WindowInteropHelper(_window).Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        if (_windowHandle == nint.Zero || _source is null || !AddClipboardFormatListener(_windowHandle))
        {
            return false;
        }

        _source.AddHook(WindowMessageHook);
        return true;
    }

    public void Dispose()
    {
        if (_source is not null)
        {
            _source.RemoveHook(WindowMessageHook);
            _source = null;
        }

        if (_windowHandle != nint.Zero)
        {
            _ = RemoveClipboardFormatListener(_windowHandle);
            _windowHandle = nint.Zero;
        }
    }

    private nint WindowMessageHook(
        nint windowHandle,
        int message,
        nint wordParameter,
        nint longParameter,
        ref bool handled)
    {
        if (message == WmClipboardUpdate)
        {
            _window.Dispatcher.BeginInvoke(ReadClipboardText, DispatcherPriority.Background);
        }

        return nint.Zero;
    }

    private void ReadClipboardText()
    {
        try
        {
            if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                _history.Add(Clipboard.GetText(TextDataFormat.UnicodeText), "Copiado manualmente");
            }
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // Another process can briefly keep the clipboard locked.
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(nint windowHandle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(nint windowHandle);
}
