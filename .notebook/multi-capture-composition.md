# Multi-capture composition
> One editor can combine independent region, window, and monitor captures

Entry: `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs`

Flow:
- The initial bitmap and every added capture are `Image` objects tagged as capture annotations on `AnnotationCanvas`.
- `+ Region`, `+ Window`, and `+ Monitor` call `MainWindow.CaptureForEditorAsync()`, which reuses the normal selectors and hides all Firaw windows before snapshotting.
- New captures are placed to the right of the existing composition. Select mode can move them; four ciano corner handles resize one selected capture while preserving the bitmap source.
- `MoveSelectedCaptureToLayer()` changes one selected print between the front and back of the capture-only layer, keeping ordinary annotations above all prints.
- `FitOutputButton_Click()` unions the visible content bounds, moves the montage to the origin, and resizes `EditorSurface` so copied and saved output has no unused margins.
- Adding, resizing, layer ordering, and output fitting participate in action-based undo/redo.
- Export renders the full `EditorSurface`, including captures and annotations. OCR and sensitive-data detection temporarily hide non-capture objects and operate on the composed captures only.

Gotchas:
- Capture images must remain below ordinary annotations in `AnnotationCanvas.Children` order.
- Selection outlines and resize handles belong to `SelectionCanvas` and must never appear in copied or saved output.
- Adding a capture must restore hidden Firaw windows even when selection is cancelled or capture fails.

Verification:
- Add overlapping captures, change their layer order, move and resize them, fit the output, then verify `Ctrl+C`, PNG export, OCR, undo, and redo against the complete composition.

Updated: 2026-09-15
