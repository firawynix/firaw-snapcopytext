using System.Windows.Input;
using Firaw.SnapCopyText.Models;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

using CaptureMode = Firaw.SnapCopyText.Models.CaptureMode;

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

    [Theory]
    [InlineData(ModifierKeys.None, CaptureMode.Region)]
    [InlineData(ModifierKeys.Alt, CaptureMode.Monitor)]
    [InlineData(ModifierKeys.Control, CaptureMode.Window)]
    public void TryGetPresetMode_MapsFirawPrintScreenProfile(
        ModifierKeys modifiers,
        CaptureMode expectedMode)
    {
        bool found = HotkeyService.TryGetPresetMode(
            new CapturePreferences(),
            modifiers,
            out CaptureMode mode);

        Assert.True(found);
        Assert.Equal(expectedMode, mode);
    }

    [Theory]
    [InlineData(ModifierKeys.None)]
    [InlineData(ModifierKeys.Alt)]
    [InlineData(ModifierKeys.Control)]
    [InlineData(ModifierKeys.Shift)]
    [InlineData(ModifierKeys.Control | ModifierKeys.Alt)]
    public void TryGetPresetMode_PassesOriginalWindowsCombinations(ModifierKeys modifiers)
    {
        var settings = new CapturePreferences
        {
            UsePrintScreen = false,
            UseAltPrintScreen = false,
            UseControlPrintScreen = false
        };

        Assert.False(HotkeyService.TryGetPresetMode(settings, modifiers, out _));
    }
}
