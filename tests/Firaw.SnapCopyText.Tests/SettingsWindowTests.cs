using System.Windows.Input;
using Firaw.SnapCopyText.Views;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class SettingsWindowTests
{
    private const int WmHotkey = 0x0312;
    private const int RecorderPrintScreenIdBase = 0x5350;

    [Fact]
    public void TryDecodeRecorderHotkey_IgnoresLargePointerFromOtherWindowMessage()
    {
        nint pointerLargerThanInt32 = unchecked((nint)0x7FFF_FFFF_FFFF_FFFFL);

        bool decoded = SettingsWindow.TryDecodeRecorderHotkey(
            message: 0x0084,
            pointerLargerThanInt32,
            out ModifierKeys modifiers);

        Assert.False(decoded);
        Assert.Equal(ModifierKeys.None, modifiers);
    }

    [Fact]
    public void TryDecodeRecorderHotkey_DecodesRegisteredPrintScreenCombination()
    {
        bool decoded = SettingsWindow.TryDecodeRecorderHotkey(
            WmHotkey,
            RecorderPrintScreenIdBase + 0x0006,
            out ModifierKeys modifiers);

        Assert.True(decoded);
        Assert.Equal(ModifierKeys.Control | ModifierKeys.Shift, modifiers);
    }

    [Fact]
    public void TryDecodeRecorderHotkey_IgnoresUnknownHotkeyId()
    {
        bool decoded = SettingsWindow.TryDecodeRecorderHotkey(
            WmHotkey,
            nint.MaxValue,
            out ModifierKeys modifiers);

        Assert.False(decoded);
        Assert.Equal(ModifierKeys.None, modifiers);
    }
}
