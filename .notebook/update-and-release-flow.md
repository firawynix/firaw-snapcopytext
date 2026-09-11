# Update and release flow
> The launcher checks a same-host manifest, verifies the installer, and falls back to the current app if the server is unavailable.

## Entry points

- `src/Firaw.SnapCopyText.Launcher/Program.cs` starts before the main WPF executable.
- `src/Firaw.SnapCopyText.Launcher/UpdateClient.cs` reads and validates the release manifest.
- `tools/build-release.ps1` produces both architectures, Inno installers, and the server-ready directory.
- `installer/firaw-snapcopytext.iss` installs the launcher and main application per user.

## Runtime flow

1. The launcher reads the installed main executable version.
2. It requests `http://10.81.66.10/firaw-snapcopytext/update.json` with a short timeout.
3. It chooses `x64` or `x86` from the manifest for the current process architecture.
4. An update is accepted only when its version is newer and its URL uses the manifest's scheme and host.
5. The installer download is checked against the declared byte size and SHA-256.
6. The verified Inno installer runs silently with `/UPDATE=1`, closes the running app, installs in place, and restarts Firaw in the tray with `--no-update` to prevent a loop.
7. Any network, manifest, or download failure is logged under `%LOCALAPPDATA%\Firaw\SnapCopyText\update.log`; the installed app still starts.

## Release layout

Running `tools\build-release.ps1 -Version <version>` creates:

```text
release/Firaw-SnapCopyText-<version>/
  installers/
  update-server/firaw-snapcopytext/
    update.json
    Firaw-SnapCopyText-Setup-x64.exe
    Firaw-SnapCopyText-Setup-x86.exe
  demonstracao-local/
```

The entire `update-server/firaw-snapcopytext` directory is the server deployment unit. Keep `update.json` and both installers together at the configured HTTP route.

## Gotchas

- Windows startup must target `Firaw.SnapCopyText.Launcher.exe`, not the WPF executable, or login startup bypasses the update check.
- The launcher accepts only packages hosted beside the manifest. A CDN or different host requires an intentional policy change.
- SHA-256 validates the downloaded file against the manifest, but an internal HTTP manifest is not equivalent to HTTPS plus code signing.
- Increment the product version before publishing; otherwise installed clients correctly ignore the release.
