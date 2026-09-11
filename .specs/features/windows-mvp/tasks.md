# Windows MVP Tasks

**Design:** `.specs/features/windows-mvp/design.md`
**Status:** Done through T29; release validation passed

## Execution Plan

T1 -> T2 -> T3 -> T4 -> T5 -> T6 -> T7 -> T8 -> T9 -> T10 -> T11 -> T12 -> T13 -> T14 -> T15 -> T16 -> T17 -> T18 -> T19 -> T20 -> T21 -> T22 -> T23 -> T24 -> T25 -> T26 -> T27 -> T28 -> T29

## Task Breakdown

### T1: Create WPF application foundation

**Status:** DONE

**Where:** project file, app entry, manifest, centralized Firaw theme, launcher window.
**Requirement:** HOTKEY-01
**Done when:** solution restores and builds an empty launcher.
**Verify:** `dotnet build Firaw.SnapCopyText.sln`

### T2: Create capture service and geometry tests

**Status:** DONE

**Where:** `Services/CaptureService.cs`, test project.
**Depends on:** T1
**Requirements:** CAP-01, CAP-02
**Done when:** virtual screen capture compiles and crop normalization tests pass.
**Verify:** `dotnet test Firaw.SnapCopyText.sln`

### T3: Create capture overlay

**Status:** DONE

**Where:** `Views/CaptureOverlayWindow.xaml(.cs)`
**Depends on:** T2
**Requirements:** CAP-01, CAP-02
**Done when:** drag returns a valid crop and Escape cancels.
**Verify:** build plus overlay logic tests.

### T4: Create annotation model and history tests

**Status:** DONE

**Where:** `Models/EditorTool.cs`, `Models/AnnotationHistory.cs`, tests.
**Depends on:** T1
**Requirements:** EDIT-01, EDIT-02
**Done when:** tool model compiles and undo/redo tests pass.
**Verify:** `dotnet test Firaw.SnapCopyText.sln`

### T5: Create editor layout

**Status:** DONE

**Where:** `Views/EditorWindow.xaml`
**Depends on:** T3, T4
**Requirements:** EDIT-01, IMG-01, IMG-02, OCR-02
**Done when:** all MVP controls render and bind to named handlers.
**Verify:** solution builds.

### T6: Implement editor interactions and image output

**Status:** DONE

**Where:** `Views/EditorWindow.xaml.cs`
**Depends on:** T5
**Requirements:** EDIT-01, EDIT-02, IMG-01, IMG-02
**Done when:** drawing, text, redaction, undo/redo, copy, and save compile and follow the spec.
**Verify:** solution builds and tests pass.

### T7: Implement local OCR and preview

**Status:** DONE

**Where:** `Services/OcrService.cs`, OCR preview dialog, project dependencies.
**Depends on:** T1
**Requirements:** OCR-01, OCR-02
**Done when:** OCR models are copied to output and recognized text can be edited and copied.
**Verify:** OCR smoke test plus solution build.

### T8: Integrate launcher and global shortcuts

**Status:** DONE

**Where:** `Services/HotkeyService.cs`, launcher code-behind.
**Depends on:** T3, T6, T7
**Requirements:** HOTKEY-01, CAP-01
**Done when:** button and available global shortcut open exactly one capture overlay.
**Verify:** solution builds and duplicate-request guard is tested.

### T9: Package documentation and validate MVP

**Status:** DONE

**Where:** `README.md`, spec/task/state status.
**Depends on:** T1-T8
**Requirements:** all
**Done when:** clean restore, build, and tests pass; run instructions are documented.
**Verify:** `dotnet restore`, `dotnet build`, and `dotnet test` all succeed.

### T10: Keep capture selection adjustable

**Status:** DONE

**Where:** ResizeHandle model, SelectionGeometry service, capture overlay, and geometry tests.
**Requirements:** CAP-01, CAP-04
**Done when:** releasing the initial drag keeps the region active, movable, resizable, and confirmable.
**Verify:** geometry tests plus overlay construction smoke test.

### T11: Exclude Firaw windows from capture

**Status:** DONE

**Where:** MainWindow capture lifecycle.
**Requirements:** CAP-03
**Done when:** all visible Firaw windows are hidden before the snapshot and restored afterward.
**Verify:** interactive test with an editor already open.

### T12: Separate full-copy and text-selection OCR flows

**Status:** DONE

**Where:** editor canvas and OCR service integration.
**Requirements:** OCR-02, OCR-03
**Done when:** Copy text copies all OCR directly and Select text lets the user drag over the image, then copies that region without opening a dialog.
**Verify:** crop/OCR tests plus interactive in-image selection and clipboard test.

### T13: Add boxed annotations

**Status:** DONE

**Where:** EditorTool, editor toolbar/canvas, and text prompt.
**Requirements:** EDIT-03
**Done when:** Annotation inserts a dark note card with the selected accent color and participates in undo/redo.
**Verify:** build plus editor construction and interaction smoke test.

### T14: Prevent window-transition capture ghosts

**Status:** DONE

**Where:** MainWindow capture lifecycle.
**Requirements:** CAP-05
**Done when:** Firaw disables hide transitions, flushes pending DWM composition, and waits before reading desktop pixels.
**Verify:** capture with an editor open and confirm no flattened window thumbnail appears.

### T15: Add Firaw Eye of Horus identity

**Status:** DONE

**Where:** application assets, project icon metadata, and launcher branding.
**Requirements:** BRAND-01
**Done when:** PNG and multi-resolution ICO assets are embedded and used by the executable and launcher.
**Verify:** release build plus visual inspection at application and tray sizes.

### T16: Keep Firaw in the system tray

**Status:** DONE

**Where:** App lifecycle and launcher capture entry point.
**Requirements:** TRAY-01, TRAY-02
**Done when:** close/minimize hides the launcher, shortcuts remain active, and tray commands open, capture, or exit.
**Verify:** background process and hidden-window shortcut smoke test.

### T17: Apply cyan title bars

**Status:** DONE

**Where:** WindowBrandingService and application window-load handler.
**Requirements:** BRAND-02
**Done when:** supported standard captions report the Firaw cyan DWM color.
**Verify:** query the live launcher caption attribute.

### T18: Monitor copied text

**Status:** DONE

**Where:** CopiedTextEntry, TextHistoryService, ClipboardMonitorService, launcher lifecycle, and unit tests.
**Requirements:** HIST-01
**Done when:** clipboard text changes are captured, deduplicated, newest-first, and limited to 100 in-memory entries.
**Verify:** text-history tests plus clipboard listener smoke test.

### T19: Add copied-text drawer

**Status:** DONE

**Where:** editor layout and interactions.
**Requirements:** HIST-02
**Done when:** the drawer opens/closes, imports the clipboard, allows extended selection, copies combined text, and clears its list.
**Verify:** editor construction plus interactive multi-select test.

### T20: Replace the launcher white background

**Status:** DONE

**Where:** launcher layout and brand documentation.
**Requirements:** BRAND-03
**Done when:** the launcher client area is cyan and its content keeps readable dark contrast.
**Verify:** rendered launcher screenshot inspection.

### T21: Add capture source modes and pickers

**Status:** DONE

**Where:** launcher, capture target models/services, target picker, capture lifecycle.
**Requirements:** CAP-06, CAP-07, CAP-08
**Done when:** Region retains the overlay, Monitor lists every screen, Window lists programs and captures the chosen handle even when covered.
**Verify:** tests, picker inspection, and interactive capture of a covered window.

### T22: Add persistent custom capture preferences

**Status:** DONE

**Where:** CapturePreferences, AppSettingsService, HotkeyService, SettingsWindow, tray menu, and tests.
**Requirements:** HOTKEY-02, HOTKEY-03
**Done when:** a recorded combination, default mode, and optional Print Screen preference survive restart and registration conflicts remain non-destructive.
**Verify:** settings and shortcut parser tests plus settings-window smoke test.

### T23: Replace color dropdown with swatch palette

**Status:** DONE

**Where:** editor footer and brush selection.
**Requirements:** EDIT-04
**Done when:** eight predefined swatches are visible and mutually exclusive, with Firaw cyan selected first.
**Verify:** editor screenshot inspection and drawing smoke test.

### T24: Increase tray-eye visual size

**Status:** DONE

**Where:** PNG and multi-resolution ICO assets.
**Requirements:** BRAND-04
**Done when:** transparent padding is reduced while the Horus silhouette remains intact at all packaged icon sizes.
**Verify:** original-resolution asset inspection and tray UAT.

### T25: Polish preferences and add Windows startup

**Status:** DONE

**Where:** preferences window, CapturePreferences, StartupService, and tests.
**Requirements:** START-01
**Done when:** preferences use readable dark/cyan contrast and a saved checkbox adds/removes the current executable from the current-user Windows startup list.
**Verify:** visual settings inspection, path quoting test, and restart UAT.

### T26: Add direct tray capture gestures and dual architecture packages

**Status:** DONE

**Where:** tray lifecycle and publish outputs.
**Requirements:** TRAY-03
**Done when:** left, Ctrl+left, middle, and right clicks have distinct documented actions, and self-contained win-x64/win-x86 packages start successfully.
**Verify:** tray interaction UAT and launch smoke test of both publish outputs.

### T27: Add the self-updating launcher

**Status:** DONE

**Where:** launcher project, update client, startup service, and tests.
**Requirements:** UPDATE-01, UPDATE-02
**Done when:** the launcher compares versions, chooses the matching architecture, verifies same-host URL, size, and SHA-256, and still opens Firaw after update failures.
**Verify:** launcher tests plus unavailable-server launch smoke test.

### T28: Build Inno release packages

**Status:** DONE

**Where:** Inno script and release build script.
**Requirements:** UPDATE-03
**Done when:** one command produces x64/x86 installers and a server-ready manifest containing their hashes and sizes.
**Verify:** compile both installers and validate the generated JSON against both files.

### T29: Create the local demonstration page

**Status:** DONE

**Where:** `demo-site/dist` and release documentation.
**Requirements:** SITE-01
**Done when:** the responsive page demonstrates capture modes, OCR, annotations, privacy, and local installer downloads without being published.
**Verify:** serve the static directory locally and inspect desktop and narrow layouts.

## Tools

- Local filesystem edits through `apply_patch`.
- Installed .NET SDK for restore, build, tests, and execution.
- Official Microsoft and Tesseract documentation for API verification.
- No external account, cloud connector, or upload service.
