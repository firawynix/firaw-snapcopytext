# Project State

## Current Focus

- Milestone M1: version 1.1.4 adds a Lightshot-style Print Screen profile with independent Region, Monitor, Window, and Windows-original routes.

## Decisions

- Product name is **Firaw - SnapCopyText**.
- The initial implementation uses WPF because it is available in the installed .NET SDK and provides direct control over transparent overlay windows, rendering, clipboard, and global hotkeys.
- OCR is local and uses Tesseract language files for Portuguese and English so the prototype does not require MSIX package identity or an NPU.
- No capture is uploaded automatically or implicitly.
- Branding follows FirawSelector: dark surfaces, high contrast, and cyan as the primary accent. The initial theme token is `#19D3E6`, centralized so the exact Firaw cyan can be adjusted once without touching individual views.
- Copied text history is kept in memory only, deduplicated, and limited to 100 entries; closing Firaw through the tray clears it.
- Region capture remains adjustable; Window and Monitor use an in-app named target picker.
- Capture preferences persist per Windows user. Print Screen combinations use a low-level keyboard hook with `RegisterHotKey` fallback, and only suppress combinations assigned to Firaw.
- Windows startup is optional and current-user scoped; the published deliverables are self-contained x64 and x86 packages.
- Tray gestures map left click to Region, Ctrl+left to Window, middle/scroll click to Monitor, and right click to the full menu.
- Installed shortcuts and Windows startup target the updater launcher; update failures never prevent the current application from opening.
- Releases and the automatic update manifest are published on GitHub over HTTPS.

## Blockers

- None.

## Next Steps

- Obtain HTTPS and a code-signing certificate before external distribution.

## Deferred Ideas

- Scrolling capture, video/GIF recording, translations, table extraction, local history, and optional sharing links.

## Lessons Learned

- The inspected Lightshot directory is a compiled installation, not a reusable source project.
- WPF `Checked` events can fire inside `InitializeComponent()` before controls declared later in XAML are assigned; initialization-time handlers must tolerate incomplete named fields.

## Quick Tasks Completed

| # | Description | Date | Commit | Status |
| --- | --- | --- | --- | --- |
| 001 | Prevent editor startup event from accessing unloaded controls | 2026-09-11 | `8cf9352` | Done |
| 002 | Add adjustable Lightshot-style selection, hide Firaw windows, and split OCR copy modes | 2026-09-11 | `546422c`, `01199ba`, `045e0d3` | Done |
