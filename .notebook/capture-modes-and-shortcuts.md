# Capture modes and shortcuts

The capture entry point receives an explicit `CaptureMode`, while global shortcuts use the saved default from `CapturePreferences`.

- Region: hide Firaw, snapshot the virtual desktop, then use the adjustable overlay.
- Monitor: choose a named `Screen` in `CaptureTargetPickerWindow`, hide Firaw, then crop its physical bounds from the virtual snapshot.
- Window: choose a visible top-level native handle, hide Firaw, call `PrintWindow(PW_RENDERFULLCONTENT)`, and fall back to the virtual-screen crop only when the target declines native rendering.

Before every pixel read, visible Firaw windows receive `WDA_EXCLUDEFROMCAPTURE`, have DWM transitions disabled, and are hidden. Their previous settings are restored after selection.

Custom shortcuts are stored as normalized labels such as `Ctrl + Alt + F9`. `HotkeyService` parses and validates the label before calling `RegisterHotKey`; incomplete chords and PrintScreen in the custom field are rejected. Print Screen has its own preference and registration result.
