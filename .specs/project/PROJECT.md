# Firaw - SnapCopyText

**Vision:** A fast Windows screenshot tool that lets people capture, annotate, copy an image, and extract text from the selected region without sending the content to a server.
**For:** Windows users who frequently move visual or textual information between applications.
**Solves:** Existing capture tools make image copying easy but make extracting text, redacting sensitive content, and keeping captures private unnecessarily slow.

## Goals

- Complete a region capture and copy the image in at most two actions after invoking the hotkey.
- Extract Portuguese and English text locally from a selected region and copy it in one action.
- Keep capture, editing, and OCR functional without an account or internet connection.

## Tech Stack

**Core:**

- Framework: WPF on .NET 9
- Language: C# 13
- OCR: Tesseract 5 with local `por` and `eng` language data
- Storage: local files only; no database in the first milestone

**Key dependencies:** `Tesseract` NuGet package, Windows desktop APIs, WPF rendering and clipboard APIs.

## Scope

**v1 includes:**

- Global capture shortcut and capture button.
- Region selection from a frozen desktop image.
- Copy image, save PNG, and copy locally recognized text.
- Pencil, arrow, rectangle, highlighter, text, redaction, color, thickness, undo, and redo.
- Portuguese and English interface and OCR defaults suitable for Brazilian users.

**Explicitly out of scope:**

- Cloud accounts, uploads, public links, and galleries.
- Scrolling capture, video/GIF recording, translation, tables-to-Excel, and browser extensions.
- Pixel-perfect support for mixed-DPI multi-monitor layouts in the first prototype.

## Constraints

- Windows-only initial release.
- Captured pixels and OCR text must remain on the local machine.
- The visual identity must follow the Firaw product line, using cyan as the primary accent over a dark neutral interface.
- This is a clean implementation and does not reuse Lightshot code, trademarks, or visual assets.
