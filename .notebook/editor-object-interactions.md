# Editor Object Interactions
> Editor annotations are movable objects with action-based undo/redo

Entry: `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs`

Flow:
- `AnnotationCanvas` owns only exportable annotations; `SelectionCanvas` owns temporary selection and eraser visuals.
- Select resolves the direct annotation under the pointer, supports a marquee for multiple objects, and moves the selected group by changing its Canvas offsets.
- History stores `EditorAction` callbacks so creation, movement, erasing, manual blur, and automatic blur all undo and redo correctly.
- Eraser Object removes the hit annotation. Circle and Square remove annotations whose bounds intersect the cursor shape; a single stroke is one history action.
- Manual blur crops the original pixels and overlays a clipped WPF `BlurEffect` object. Automatic protection obtains Tesseract text-line bounds, filters them with `SensitiveDataDetector`, then creates the same movable blur objects.
- Export temporarily hides `SelectionCanvas`, keeping selection outlines and eraser previews out of copied/saved images.

Gotchas:
- Keep OCR on `OriginalImage`, not the rendered editor, so annotations do not pollute sensitive-data detection.
- Treat automatic detection as a reviewable aid: false positives/negatives remain possible, and every generated blur is undoable/movable.
- Controls that fire during XAML initialization must not overwrite editor status unless their tool is active.

Verification:
- Unit tests cover OCR regions and sensitive-data patterns.
- Visual smoke test should create a blur, select and move it, marquee-select it, erase it, and undo the erase.

Updated: 2026-09-11
