# Editor Object Interactions
> Editor annotations are movable objects with action-based undo/redo

Entry: `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs`

Flow:
- `AnnotationCanvas` owns exportable captures and annotations; `SelectionCanvas` owns temporary selection, resize handles, and eraser visuals.
- Select resolves the direct annotation under the pointer, supports a marquee for multiple objects, and moves the selected group by changing its Canvas offsets.
- History stores `EditorAction` callbacks so creation, movement, clipping with the eraser, deletion, transformation, manual blur, and automatic blur all undo and redo correctly.
- Circle and Square erasers subtract a widened pointer path from each affected annotation's local `UIElement.Clip`; they erase only the touched pixels, including captures, and a complete drag is one history action. The white `EditorSurface` remains visible below an erased capture.
- Select offers four corner resize handles and an orange rotation handle for any single object. Resizing/rotation use `RenderTransform`; `Delete` removes the selected object(s), with undo/redo.
- Manual blur crops the capture-only composition and overlays a clipped WPF `BlurEffect` object. Automatic protection obtains Tesseract text-line bounds from that composition, filters them with `SensitiveDataDetector`, then creates the same movable blur objects.
- Export temporarily hides `SelectionCanvas`, keeping selection outlines and eraser previews out of copied/saved images.
- The ten tools are numbered 1-9 and 0 in panel order. `TrySelectToolByNumber()`
  maps both the digit row and numpad to the same toggle buttons.
- The first capture sizes the editor to its exact pixel dimensions, capped to
  the current working area. `FitEditorToCapture()` applies a layout scale only
  when necessary; export still renders the complete `EditorSurface` at its
  composition resolution.

Gotchas:
- Keep OCR on the capture-only composition, not the fully rendered editor, so annotations do not pollute sensitive-data detection while additional prints remain recognizable.
- Treat automatic detection as a reviewable aid: false positives/negatives remain possible, and every generated blur is undoable/movable.
- Controls that fire during XAML initialization must not overwrite editor status unless their tool is active.

Verification:
- Unit tests cover OCR regions and sensitive-data patterns.
- Visual smoke test should create a blur, select/move/resize/rotate it, erase part of it and of a capture with a dragging stroke, undo/redo that stroke, and remove a selected edit with `Delete` (also undoable).

Updated: 2026-09-22
