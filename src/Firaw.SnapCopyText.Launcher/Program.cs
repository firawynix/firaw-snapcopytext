using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Firaw.SnapCopyText.Launcher;

internal static class Program
{
    private const string ApplicationFileName = "Firaw.SnapCopyText.exe";

    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        string applicationPath = Path.Combine(AppContext.BaseDirectory, ApplicationFileName);
        string[] applicationArguments = args
            .Where(argument => !argument.Equals("--no-update", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (!File.Exists(applicationPath))
        {
            WriteLog($"Aplicativo não encontrado: {applicationPath}");
            return 2;
        }

        if (!args.Contains("--no-update", StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                bool updateStarted = await TryStartUpdateAsync(applicationPath);
                if (updateStarted)
                {
                    return 0;
                }
            }
            catch (Exception exception)
            {
                WriteLog($"Falha ao verificar atualização: {exception}");
            }
        }

        return StartApplication(applicationPath, applicationArguments) ? 0 : 3;
    }

    private static async Task<bool> TryStartUpdateAsync(string applicationPath)
    {
        Version currentVersion = ReadCurrentVersion(applicationPath);
        using var httpClient = new HttpClient { Timeout = UpdateConfiguration.RequestTimeout };
        var updateClient = new UpdateClient(httpClient);
        UpdatePlan? plan = await updateClient.FindUpdateAsync(
            UpdateConfiguration.ManifestUri,
            currentVersion,
            RuntimeInformation.ProcessArchitecture);
        if (plan is null)
        {
            return false;
        }

        string updatesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Firaw",
            "SnapCopyText",
            "Updates");
        string packageName = Path.GetFileName(plan.PackageUri.AbsolutePath);
        string installerPath = Path.Combine(updatesDirectory, packageName);
        await updateClient.DownloadAndVerifyAsync(plan, installerPath);

        string installDirectory = Path.GetDirectoryName(applicationPath)!;
        var installer = new ProcessStartInfo(installerPath)
        {
            UseShellExecute = true,
            Arguments = $"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS /UPDATE=1 /DIR=\"{installDirectory}\""
        };
        Process.Start(installer);
        WriteLog($"Atualização {plan.Version} iniciada por {packageName}.");
        return true;
    }

    private static Version ReadCurrentVersion(string applicationPath)
    {
        string? version = FileVersionInfo.GetVersionInfo(applicationPath).FileVersion;
        return Version.TryParse(version, out Version? parsed) ? parsed : new Version(0, 0);
    }

    private static bool StartApplication(string applicationPath, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(applicationPath) { UseShellExecute = true };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return Process.Start(startInfo) is not null;
    }

    private static void WriteLog(string message)
    {
        try
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Firaw",
                "SnapCopyText");
            Directory.CreateDirectory(directory);
            File.AppendAllText(
                Path.Combine(directory, "update.log"),
                $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging cannot prevent the current installed version from opening.
        }
    }
}
