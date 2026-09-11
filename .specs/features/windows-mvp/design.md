# Windows MVP Design

**Spec:** `.specs/features/windows-mvp/spec.md`
**Status:** Approved from the user's instruction to proceed with the recommended MVP.

## Architecture Overview

The WPF process owns a small launcher window and registers capture hotkeys. Before the capture service snapshots the virtual desktop, the launcher records and hides every visible Firaw window, waits for the UI compositor to settle, and restores the same windows after selection. The borderless overlay keeps the selected rectangle active so it can be moved or resized through eight handles before confirmation. The editor renders annotations over the image, delegates OCR to a local service, and writes image or Unicode text to the clipboard.

The presentation layer uses centralized Firaw theme resources: dark neutral backgrounds, white text, and cyan (`#19D3E6`) for primary actions, focus, selection borders, and active tools.

Flow: Hotkey/Button -> Hide Firaw windows -> Desktop capture -> Adjustable selection overlay -> Restore windows -> Editor -> Clipboard/PNG or Local OCR -> Direct copy or selectable text -> Clipboard.

## Code Reuse Analysis

This is a greenfield project. It reuses Windows desktop, WPF rendering, clipboard, and dialog APIs. No Lightshot binaries or assets are referenced.

## Components

### CaptureService

- **Purpose:** Snapshot the virtual desktop and crop a normalized pixel rectangle.
- **Location:** `src/Firaw.SnapCopyText/Services/CaptureService.cs`
- **Interfaces:** `CaptureVirtualScreen()` and `Crop(BitmapSource, Int32Rect)`.

### CaptureOverlayWindow

- **Purpose:** Present the frozen desktop, retain a live selection with movement and eight-direction resizing, and return the confirmed crop.
- **Location:** `src/Firaw.SnapCopyText/Views/CaptureOverlayWindow.xaml(.cs)`
- **Dependencies:** CaptureService output, SelectionGeometry, and WPF mouse/Thumb input.

### EditorWindow

- **Purpose:** Display the crop, create annotations, render output, and expose copy/save/OCR actions.
- **Location:** `src/Firaw.SnapCopyText/Views/EditorWindow.xaml(.cs)`
- **Dependencies:** AnnotationHistory and OcrService.

### OcrService

- **Purpose:** Recognize Portuguese and English text from an in-memory PNG using local language models.
- **Location:** `src/Firaw.SnapCopyText/Services/OcrService.cs`
- **Dependencies:** Tesseract and `tessdata` files copied beside the executable.

### HotkeyService

- **Purpose:** Register Print Screen and a fallback Ctrl+Shift+S shortcut and raise capture requests.
- **Location:** `src/Firaw.SnapCopyText/Services/HotkeyService.cs`
- **Dependencies:** Win32 `RegisterHotKey` and the main window handle.

## Data Models

- `EditorTool`: current drawing mode.
- `AnnotationHistory`: applied and redo element stacks.
- `CaptureResult`: cropped bitmap plus screen location metadata.

## Error Handling Strategy

| Scenario | Handling | User impact |
| --- | --- | --- |
| Hotkey conflict | Register fallback and show status | Capture button remains available |
| Empty selection | Close overlay without editor | No error dialog |
| OCR model missing | Return actionable local error | Image editing remains usable |
| Clipboard temporarily busy | Retry briefly, then show message | No data loss |

## Tech Decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Desktop UI | WPF on .NET 9 | Installed toolchain, mature overlay/rendering APIs |
| Region capture | Frozen virtual-screen bitmap | Matches the fast Lightshot-style drag interaction |
| OCR | Tesseract 5 local models | Offline, no NPU requirement, no automatic upload |
| Output | WPF RenderTargetBitmap | Captures the base image and annotations together |
