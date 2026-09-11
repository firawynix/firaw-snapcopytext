using System.Windows.Input;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class HotkeyServiceTests
{
    [Fact]
    public void TryCreateShortcutLabel_BuildsCustomCombination()
    {
        bool valid = HotkeyService.TryCreateShortcutLabel(
            ModifierKeys.Control | ModifierKeys.Alt,
            Key.F9,
            out string label);

        Assert.True(valid);
        Assert.Equal("Ctrl + Alt + F9", label);
    }

    [Fact]
    public void TryParseShortcut_NormalizesSavedCombination()
    {
        bool valid = HotkeyService.TryParseShortcut(
            "ctrl+shift+a",
            out uint modifiers,
            out Key key,
            out string label);

        Assert.True(valid);
        Assert.NotEqual(0u, modifiers);
        Assert.Equal(Key.A, key);
        Assert.Equal("Ctrl + Shift + A", label);
    }

    [Theory]
    [InlineData("PrintScreen")]
    [InlineData("Print Screen")]
    [InlineData("PrtSc")]
    public void TryParseShortcut_AcceptsPrintScreenWithoutModifiers(string shortcut)
    {
        bool valid = HotkeyService.TryParseShortcut(
            shortcut,
            out uint modifiers,
            out Key key,
            out string label);

        Assert.True(valid);
        Assert.Equal(0u, modifiers);
        Assert.Equal(Key.PrintScreen, key);
        Assert.Equal("Print Screen", label);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("Ctrl + Shift")]
    public void TryParseShortcut_RejectsUnsafeOrIncompleteShortcut(string shortcut)
    {
        Assert.False(HotkeyService.TryParseShortcut(shortcut, out _, out _, out _));
    }
}
