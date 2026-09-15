# Capture modes and shortcuts

The capture entry point receives an explicit `CaptureMode`, while global shortcuts use the saved default from `CapturePreferences`.

- Region: hide Firaw, snapshot the virtual desktop, then use the adjustable overlay.
- Monitor: choose a named `Screen` in `CaptureTargetPickerWindow`, hide Firaw, then crop its physical bounds from the virtual snapshot.
- Window: choose a visible top-level native handle, hide Firaw, call `PrintWindow(PW_RENDERFULLCONTENT)`, and fall back to the virtual-screen crop only when the target declines native rendering.

Before every pixel read, visible Firaw windows receive `WDA_EXCLUDEFROMCAPTURE`, have DWM transitions disabled, and are hidden. Their previous settings are restored after selection.

Capture confirmation returns both the selected target and a
`CaptureResultAction`. Region, Window, and Monitor can use `OpenEditor` or
`CopyImage`; the latter copies only after the selection UI has closed and the
hidden Firaw windows have been restored. The region overlay and target picker
expose a **Copy image** button and handle Ctrl+C. The editor handles Ctrl+C as
the same action as its existing **Copy image** button. The shared target picker
handles Escape as `DialogResult = false`, so Window and Monitor cancel through
the same path as the **Cancel** button.

The main window exposes Region, Window, and Monitor as numbered options 1-3 and
handles the corresponding digit or numpad key. After a region is drawn, the
overlay exposes 1 Reselect, 2 Copy, and 3 Open editor. Reselect calls
`ClearSelection()` and keeps the captured desktop open, so a replacement region
does not require another desktop snapshot.

Custom shortcuts are stored as normalized labels such as `Ctrl + Alt + F9`. `HotkeyService` parses and validates the label before calling `RegisterHotKey`; ordinary keys still require Ctrl, Shift, or Alt, while the dedicated Print Screen key and function keys may be used alone. Print Screen accepts `PrintScreen`, `Print Screen`, and `PrtSc`, normalizes them to `Print Screen`, and avoids duplicate registration when the separate Print Screen preference is also enabled.

Print Screen profile: `HotkeyService.KeyboardProcedure()` maps the exact combinations Print Screen → Region, Alt+Print Screen → Monitor, and Ctrl+Print Screen → Window. `WH_KEYBOARD_LL` is primary so Windows reservations do not prevent capture; `RegisterHotKey` is the fallback if the hook cannot be installed. A combination is suppressed only when its matching `CapturePreferences` flag is enabled; disabled combinations continue through `CallNextHookEx` for native Windows behavior. The custom shortcut remains a separate default-mode fallback.

Windows integration: `WindowsPrintScreenService.SetScreenSnippingEnabled()` changes the current-user `PrintScreenKeyForSnippingEnabled` value. Settings expose both value directions plus per-combination Firaw/Windows radio choices.

Settings recording: `MainWindow.OpenSettings()` suspends current global hotkeys before showing the modal dialog and restores them in `finally`. `SettingsWindow.OnSourceInitialized()` owns a temporary foreground Print Screen registration while the dialog is open; its window hook records the key when the shortcut field has focus. `PreviewKeyUp` is a fallback for keyboards that expose Print Screen only on release.

Updated: 2026-09-15
