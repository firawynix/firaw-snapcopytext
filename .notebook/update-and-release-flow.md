# Update and release flow
> The launcher checks the latest GitHub release manifest, verifies the installer, and falls back to the current app if GitHub is unreachable.

## Entry points

- `src/Firaw.SnapCopyText.Launcher/Program.cs` starts before the main WPF executable.
- `src/Firaw.SnapCopyText.Launcher/UpdateConfiguration.cs` holds the manifest URL:
  `https://github.com/firawynix/firaw-snapcopytext/releases/latest/download/update.json`.
- `src/Firaw.SnapCopyText.Launcher/UpdateClient.cs` reads and validates the release manifest.
- `tools/build-release.ps1` produces both architectures, Inno installers, `.sha256` files and the manifest.
- `installer/firaw-snapcopytext.iss` installs the launcher and main application per user.

## Runtime flow

1. The launcher reads the installed main executable `FileVersion`.
2. It requests the manifest from the latest GitHub release with a short timeout.
3. It chooses `x64` or `x86` from the manifest for the current process architecture.
4. An update is accepted only when its version is newer and its URL uses the manifest's scheme and host (`https://github.com`). The redirect to GitHub's asset storage happens inside `HttpClient`, after that check.
5. The installer download is checked against the declared byte size and SHA-256.
6. The verified Inno installer runs silently with `/UPDATE=1`, closes the running app, installs in place, and restarts Firaw in the tray with `--no-update` to prevent a loop.
7. Any network, manifest, or download failure is logged under `%LOCALAPPDATA%\Firaw\SnapCopyText\update.log`; the installed app still starts.

## Release layout

Running `tools\build-release.ps1 -Version <version>` creates:

```text
release/Firaw-SnapCopyText-<version>/
  installers/
  github-release/
    update.json
    Firaw-SnapCopyText-Setup-x64.exe (+ .sha256)
    Firaw-SnapCopyText-Setup-x86.exe (+ .sha256)
```

Publish the five files of `github-release/` to the tag `v<version>`
(`gh release create`). The manifest points to the installers of that same tag, so
the manifest and the installer never drift apart when the next release appears.

## Other channels

- Site `https://snapcopytext.firawynix.com.br` (`demo-site/`) links to
  `releases/latest/download/...`; it never stores installers.
- Firawynix Center (games/projects launcher): the catalog entry `snapcopytext`
  is refreshed every 15 minutes from the latest release by a timer on the server
  (`firawynix/firawynix-center`, `tools/`). It trusts `<installer>.sha256` and
  requires it to match GitHub's asset digest.
- After publishing a release, start `sync-releases-center.service` on srv1 when
  the catalog must update immediately; otherwise the visible version can lag by
  up to 15 minutes even though the timer is healthy.

## Gotchas

- Windows startup must target `Firaw.SnapCopyText.Launcher.exe`, not the WPF executable, or login startup bypasses the update check. Uninstall removes the `HKCU\...\Run` value (`[Code]` in the `.iss`).
- The launcher accepts only packages on the manifest's host. A CDN or different host requires an intentional policy change.
- Bump the version in both `.csproj` files AND pass it to the build: the launcher compares the `FileVersion` of the installed `.exe`. A `.exe` still reporting the old version would be reinstalled on every start.
- `tools/build-release.ps1` is saved as UTF-8 **with BOM**: Windows PowerShell 5.1 reads BOM-less scripts as ANSI and garbles the accented release notes inside `update.json`.
- WPF hooks receive pointer-sized `wParam` values for every native window message. Filter the message first and use `ToInt64()` plus range validation; calling `IntPtr.ToInt32()` before checking `WM_HOTKEY` crashes the x64 app and can make Windows PCA show a misleading compatibility/TLS warning.
