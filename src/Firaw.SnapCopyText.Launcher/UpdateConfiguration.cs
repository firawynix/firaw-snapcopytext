namespace Firaw.SnapCopyText.Launcher;

public static class UpdateConfiguration
{
    public static readonly Uri ManifestUri = new(
        "http://10.81.66.10/firaw-snapcopytext/update.json",
        UriKind.Absolute);

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
}
