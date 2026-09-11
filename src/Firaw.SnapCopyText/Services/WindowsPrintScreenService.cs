using Microsoft.Win32;

namespace Firaw.SnapCopyText.Services;

public static class WindowsPrintScreenService
{
    private const string KeyboardKeyPath = @"Control Panel\Keyboard";
    private const string ScreenSnippingValueName = "PrintScreenKeyForSnippingEnabled";

    public static void SetScreenSnippingEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyboardKeyPath, writable: true)
            ?? throw new InvalidOperationException("Não foi possível abrir as configurações de teclado do Windows.");
        key.SetValue(ScreenSnippingValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
    }
}
