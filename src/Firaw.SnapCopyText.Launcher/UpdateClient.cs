using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Firaw.SnapCopyText.Launcher;

public sealed class UpdateClient(HttpClient httpClient)
{
    public async Task<UpdatePlan?> FindUpdateAsync(
        Uri manifestUri,
        Version currentVersion,
        Architecture architecture,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(manifestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);

        JsonElement root = document.RootElement;
        if (!Version.TryParse(root.GetProperty("version").GetString(), out Version? availableVersion) ||
            availableVersion <= currentVersion)
        {
            return null;
        }

        string architectureName = architecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => throw new PlatformNotSupportedException("O atualizador Firaw suporta pacotes x64 e x86.")
        };

        JsonElement package = root.GetProperty("packages").GetProperty(architectureName);
        string packageLocation = package.GetProperty("url").GetString() ?? string.Empty;
        var packageUri = new Uri(manifestUri, packageLocation);
        string sha256 = (package.GetProperty("sha256").GetString() ?? string.Empty).ToUpperInvariant();
        long size = package.TryGetProperty("size", out JsonElement sizeElement)
            ? sizeElement.GetInt64()
            : 0;

        if (packageUri.Scheme != manifestUri.Scheme ||
            !packageUri.Host.Equals(manifestUri.Host, StringComparison.OrdinalIgnoreCase) ||
            !IsSha256(sha256))
        {
            throw new InvalidDataException("O manifesto de atualização contém um pacote inválido.");
        }

        string notes = root.TryGetProperty("releaseNotes", out JsonElement notesElement)
            ? notesElement.GetString() ?? string.Empty
            : string.Empty;
        return new UpdatePlan(availableVersion, notes, packageUri, sha256, size);
    }

    public async Task DownloadAndVerifyAsync(
        UpdatePlan plan,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        string temporaryPath = destinationPath + ".download";
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                plan.PackageUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            await using (Stream source = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (FileStream destination = File.Create(temporaryPath))
            {
                await source.CopyToAsync(destination, cancellationToken);
            }

            long downloadedSize = new FileInfo(temporaryPath).Length;
            if (plan.Size > 0 && downloadedSize != plan.Size)
            {
                throw new InvalidDataException("O tamanho do instalador baixado não confere.");
            }

            string downloadedHash = await ComputeSha256Async(temporaryPath, cancellationToken);
            if (!downloadedHash.Equals(plan.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new CryptographicException("A assinatura SHA-256 da atualização não confere.");
            }

            File.Move(temporaryPath, destinationPath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    public static async Task<string> ComputeSha256Async(
        string path,
        CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(path);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(character => char.IsAsciiHexDigit(character));
}
