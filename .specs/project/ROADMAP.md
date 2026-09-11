# Roadmap

**Current Milestone:** M1 - Working Windows MVP
**Status:** In Progress - automated validation complete; interactive UAT pending

## M1 - Working Windows MVP

**Goal:** Ship a locally runnable application that captures a region, annotates it, copies the image, saves PNG, and copies OCR text.
**Target:** All P1 requirements build successfully and pass automated smoke checks.

### Features

**Capture and selection** - COMPLETE

- Main capture action and global shortcut.
- Frozen-screen region selection with movement and eight resize handles.
- Firaw windows hidden before the desktop snapshot.
- Cancel and retry flows.

**Instant editor** - COMPLETE

- Basic annotations, redaction, colors, thickness, undo, and redo.
- Dark boxed notes with the current Firaw accent color.
- Copy rendered result or save as PNG.

**Local OCR** - COMPLETE

- Portuguese and English recognition.
- One-click copy of all recognized text.
- Cyan in-image region selection to copy only the desired text without a dialog.

## M2 - Daily-use polish

**Goal:** Make Firaw comfortable as a full-time screenshot utility.

### Features

**System tray and preferences** - PLANNED
**Local capture history** - PLANNED
**Window and full-screen capture modes** - PLANNED
**Mixed-DPI multi-monitor hardening** - PLANNED

## Future Considerations

- Scrolling capture.
- QR and barcode recognition.
- Structured table extraction.
- Screen recording and GIF export.
- Optional, explicit cloud sharing.
