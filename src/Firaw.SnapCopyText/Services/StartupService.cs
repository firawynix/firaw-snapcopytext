using Microsoft.Win32;
using System.IO;

namespace Firaw.SnapCopyText.Services;

public sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Firaw SnapCopyText";

    public void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (!enabled)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            return;
        }

        string executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Não foi possível localizar o executável do Firaw.");
        key.SetValue(ValueName, BuildStartupCommand(ResolveStartupExecutable(executablePath)), RegistryValueKind.String);
    }

    public static string ResolveStartupExecutable(string applicationExecutablePath)
    {
        string? directory = Path.GetDirectoryName(applicationExecutablePath);
        string launcherPath = Path.Combine(directory ?? string.Empty, "Firaw.SnapCopyText.Launcher.exe");
        return File.Exists(launcherPath) ? launcherPath : applicationExecutablePath;
    }

    public static string BuildStartupCommand(string executablePath) =>
        $"\"{executablePath.Trim('"')}\" --background";
}
