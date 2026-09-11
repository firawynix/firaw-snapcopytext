# Windows MVP Design

**Spec:** `.specs/features/windows-mvp/spec.md`
**Status:** Approved from the user's instruction to proceed with the recommended MVP.

## Architecture Overview

The WPF process owns a small launcher window and registers user-configurable capture hotkeys. Region capture uses the adjustable borderless overlay. Window and monitor capture first open a Firaw target picker containing the available programs or screens. Before pixels are read, the launcher records every visible Firaw window, marks it excluded from capture, disables DWM transitions, hides it, waits for the compositor, and restores the original state afterward. Window capture uses the selected native handle so overlapping foreground windows are not flattened into the result; screen crop remains the compatibility fallback. The editor renders annotations over the image, delegates OCR to a local service, and writes image or Unicode text to the clipboard.

The presentation layer uses centralized Firaw theme resources: dark neutral backgrounds, white text, and cyan (`#19D3E6`) for primary actions, focus, selection borders, and active tools.

Flow: Hotkey/Button -> Default or explicit capture mode -> Optional in-app target picker -> Hide Firaw windows -> Region overlay / monitor crop / native window render -> Restore windows -> Editor -> Clipboard/PNG or local OCR.

Background flow: Windows clipboard update -> hidden launcher window message hook -> in-memory TextHistoryService -> editor drawer -> multi-selection -> combined clipboard text.

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
- **Text-region interaction:** Select text activates a temporary cyan rectangle on the editor canvas. The rectangle is removed on release, its underlying original-image pixels are cropped, and OCR output is copied without a preview dialog.
- **Boxed annotation:** Annotation reuses the text prompt and inserts a dark semi-transparent card with the current color applied to its border and text.

### OcrService

- **Purpose:** Recognize Portuguese and English text from an in-memory PNG using local language models.
- **Location:** `src/Firaw.SnapCopyText/Services/OcrService.cs`
- **Dependencies:** Tesseract and `tessdata` files copied beside the executable.

### HotkeyService

- **Purpose:** Register optional Print Screen plus the user's recorded combination and raise capture requests in the saved default mode.
- **Location:** `src/Firaw.SnapCopyText/Services/HotkeyService.cs`
- **Dependencies:** Win32 `RegisterHotKey` and the main window handle.

### ClipboardMonitorService and TextHistoryService

- **Purpose:** Receive WM_CLIPBOARDUPDATE while the launcher is visible or hidden, deduplicate up to 100 text entries in memory, and expose them to every editor.
- **Location:** `Services/ClipboardMonitorService.cs`, `Services/TextHistoryService.cs`.
- **Dependencies:** Win32 clipboard format listener and the persistent launcher window handle.

### App tray lifecycle

- **Purpose:** Keep the process alive explicitly, hide the launcher on close/minimize, expose the full context menu, map left/Ctrl+left/middle clicks to Region/Window/Monitor, and perform intentional shutdown.
- **Location:** `App.xaml.cs`.
- **Dependencies:** Windows Forms NotifyIcon using the multi-resolution Firaw ICO resource.

### WindowBrandingService

- **Purpose:** Apply Firaw cyan caption and dark caption text through supported Windows DWM attributes.
- **Location:** `Services/WindowBrandingService.cs`.

### AppSettingsService and capture target services

- **Purpose:** Persist per-user capture preferences, enumerate visible windows and monitors, and expose named/dimensioned targets to the picker.
- **Location:** `Services/AppSettingsService.cs`, `Services/WindowSelectionService.cs`, and `Views/CaptureTargetPickerWindow.xaml(.cs)`.
- **Storage:** `%LOCALAPPDATA%\Firaw\SnapCopyText\settings.json`; clipboard text history remains memory-only.

### StartupService

- **Purpose:** Add or remove the exact running executable from the current user's Windows Run entry.
- **Location:** `Services/StartupService.cs`.
- **Scope:** Current user only, avoiding administrator elevation; the selected architecture registers its own executable path.

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
| Covered window capture | Win32 PrintWindow with visible-screen fallback | Requests the selected window's pixels instead of flattening windows in front |
| Preferences | Small per-user JSON file | Keeps default mode, custom shortcut, and Print Screen choice across restarts |
| OCR | Tesseract 5 local models | Offline, no NPU requirement, no automatic upload |
| Output | WPF RenderTargetBitmap | Captures the base image and annotations together |
