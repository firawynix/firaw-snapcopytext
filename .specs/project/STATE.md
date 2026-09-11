# Project State

## Current Focus

- Milestone M1: adjustable capture and full Firaw window hiding published; in-image OCR selection implemented and awaiting publication/UAT.

## Decisions

- Product name is **Firaw - SnapCopyText**.
- The initial implementation uses WPF because it is available in the installed .NET SDK and provides direct control over transparent overlay windows, rendering, clipboard, and global hotkeys.
- OCR is local and uses Tesseract language files for Portuguese and English so the prototype does not require MSIX package identity or an NPU.
- No capture is uploaded automatically or implicitly.
- Branding follows FirawSelector: dark surfaces, high contrast, and cyan as the primary accent. The initial theme token is `#19D3E6`, centralized so the exact Firaw cyan can be adjusted once without touching individual views.

## Blockers

- None.

## Next Steps

- User validates that Firaw windows disappear before capture, the selected region moves/resizes, and both OCR copy modes work.
- Adjust `FirawCyanColor` if the exact FirawSelector cyan differs from the initial `#19D3E6` token.

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
