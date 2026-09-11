# Project State

## Current Focus

- Milestone M1: automated validation complete; interactive UI acceptance test pending.

## Decisions

- Product name is **Firaw - SnapCopyText**.
- The initial implementation uses WPF because it is available in the installed .NET SDK and provides direct control over transparent overlay windows, rendering, clipboard, and global hotkeys.
- OCR is local and uses Tesseract language files for Portuguese and English so the prototype does not require MSIX package identity or an NPU.
- No capture is uploaded automatically or implicitly.
- Branding follows FirawSelector: dark surfaces, high contrast, and cyan as the primary accent. The initial theme token is `#19D3E6`, centralized so the exact Firaw cyan can be adjusted once without touching individual views.

## Blockers

- None.

## Next Steps

- User validates capture selection, annotations, clipboard, and OCR in the published build.
- Adjust `FirawCyanColor` if the exact FirawSelector cyan differs from the initial `#19D3E6` token.

## Deferred Ideas

- Scrolling capture, video/GIF recording, translations, table extraction, local history, and optional sharing links.

## Lessons Learned

- The inspected Lightshot directory is a compiled installation, not a reusable source project.
