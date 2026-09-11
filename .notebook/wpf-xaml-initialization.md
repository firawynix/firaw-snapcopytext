# WPF XAML Initialization
> Named controls may be incomplete when property-triggered events fire

Entry: `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs:EditorWindow()` (L35-42)

Gotcha: `IsChecked="True"` invokes `EditorWindow.ToolButton_Checked()` during `InitializeComponent()`.
- Controls declared later in XAML can still be null.
- Guard initialization-time dependencies before reading `EditorStatus` or peer controls.
- Regression verification: construct and show `EditorWindow` with a real `BitmapSource`.

Fix: `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs:ToolButton_Checked()` (L44-63)

Updated: 2026-09-11
