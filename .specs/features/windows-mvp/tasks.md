# Windows MVP Tasks

**Design:** `.specs/features/windows-mvp/design.md`
**Status:** Done through T14; interactive UAT pending

## Execution Plan

T1 -> T2 -> T3 -> T4 -> T5 -> T6 -> T7 -> T8 -> T9 -> T10 -> T11 -> T12 -> T13 -> T14

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

## Tools

- Local filesystem edits through `apply_patch`.
- Installed .NET SDK for restore, build, tests, and execution.
- Official Microsoft and Tesseract documentation for API verification.
- No external account, cloud connector, or upload service.
