# Clipboard History Flow
> Hidden launcher receives clipboard updates and feeds every editor drawer

Entry: `src/Firaw.SnapCopyText/MainWindow.xaml.cs:MainWindow_SourceInitialized()`
Flow: `ClipboardMonitorService.WindowMessageHook()` → `TextHistoryService.Add()` → shared ObservableCollection → editor drawer

Lifecycle:
- Listener uses the launcher HWND and remains registered while the launcher is hidden in the tray.
- `App.ExitApplication()` closes the launcher, which disposes the listener.
- Entries are in memory only, newest first, deduplicated, maximum 100.
- Editor drawer uses extended ListBox selection and `TextHistoryService.Combine()`.

Updated: 2026-09-11
