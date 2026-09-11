# Capture Window Visibility
> Snapshot only after every visible Firaw window has left the desktop

Entry: MainWindow.BeginCaptureAsync()

Gotcha: hiding only MainWindow does not exclude editor or OCR windows that belong to the same process.
- Record all visible windows from Application.Current.Windows before capture.
- Hide owned windows before their owner, then wait for the dispatcher and compositor before calling CopyFromScreen.
- Temporarily force-disable each window's DWM transitions before Hide, call DwmFlush after the dispatcher yields, and leave a short stabilization delay. This prevents a shrinking window thumbnail from entering the snapshot.
- Restore owners before owned windows and reactivate the window that was active.
- Restore the transition state that each window had before capture.
- Keep restoration in success, failure, and final cleanup paths.

Updated: 2026-09-11
