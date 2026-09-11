namespace Firaw.SnapCopyText.Launcher;

public sealed record UpdatePlan(
    Version Version,
    string ReleaseNotes,
    Uri PackageUri,
    string Sha256,
    long Size);
