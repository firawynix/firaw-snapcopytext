namespace Firaw.SnapCopyText.Launcher;

public static class UpdateConfiguration
{
    // Release mais recente do GitHub, por HTTPS. O manifesto aponta para os
    // instaladores da MESMA tag (github.com também): o UpdateClient recusa pacote
    // em outro host, e o redirecionamento para o armazenamento do GitHub acontece
    // dentro do HttpClient, depois dessa conferência.
    public static readonly Uri ManifestUri = new(
        "https://github.com/firawynix/firaw-snapcopytext/releases/latest/download/update.json",
        UriKind.Absolute);

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
}
