# Capture Window Visibility
> Snapshot only after every visible Firaw window has left the desktop

Entry: MainWindow.BeginCaptureAsync()

Gotcha: hiding only MainWindow does not exclude editor or OCR windows that belong to the same process.
- Record all visible windows from Application.Current.Windows before capture.
- Hide owned windows before their owner, then wait for the dispatcher and compositor before calling CopyFromScreen.
- A flattened miniature of a Firaw window in repeated captures is consistent with a DWM hide-transition frame. Mitigation: temporarily force-disable each window's transitions before Hide, call DwmFlush after the dispatcher yields, and leave a short stabilization delay. Confirm the pixel-level result in interactive UAT.
- Restore owners before owned windows and reactivate the window that was active.
- Restore the transition state that each window had before capture.
- Keep restoration in success, failure, and final cleanup paths.

Updated: 2026-09-11
