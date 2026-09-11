# Windows MVP Specification

## Problem Statement

Users need to capture a screen region, mark it up, and reuse either the image or its text immediately. Existing lightweight tools copy images well but do not make private, local text extraction a primary action.

## Goals

- [x] Capture and open a selected region in the editor.
- [x] Copy edited pixels or locally recognized text without network access.
- [x] Provide the most useful annotation and redaction tools without slowing the capture flow.

## Out of Scope

| Feature | Reason |
| --- | --- |
| Cloud upload | Privacy-first v1 |
| History database | Not required to prove the core workflow |
| Video and scrolling capture | Separate capture pipelines |

## User Stories

### P1: Select a screen region

**User Story:** As a Windows user, I want to invoke Firaw and drag over a region so that I can work only with the content I need.

**Acceptance Criteria:**

1. WHEN the user invokes capture THEN Firaw SHALL hide every visible Firaw window before freezing the desktop.
2. WHEN the user releases a non-empty initial rectangle THEN Firaw SHALL keep the selection active.
3. WHEN the user drags the selection or one of its eight handles THEN Firaw SHALL move or resize it within the virtual desktop.
4. WHEN the user confirms by button or Enter THEN Firaw SHALL open the adjusted crop in the editor.
5. WHEN the user presses Escape THEN Firaw SHALL cancel safely and restore the previously visible Firaw windows.
6. WHEN Firaw windows are hidden THEN Firaw SHALL disable their DWM transitions and wait for composition so no shrinking-window ghost is captured.

**Independent Test:** Leave an editor open, invoke capture, drag and release, move and resize the region, then confirm that no Firaw window appears in the crop.

### P1: Copy or save the image

**User Story:** As a user, I want to annotate and copy or save the result so that I can use it in another application.

**Acceptance Criteria:**

1. WHEN the user draws an annotation THEN Firaw SHALL render it above the capture.
2. WHEN the user chooses Copy image THEN Firaw SHALL place the rendered image on the Windows clipboard.
3. WHEN the user chooses Save THEN Firaw SHALL write a PNG selected by the user.
4. WHEN the user chooses Undo or Redo THEN Firaw SHALL update the latest annotation accordingly.
5. WHEN the user chooses Annotation and clicks the image THEN Firaw SHALL add the entered note as a dark box with a cyan-colored border and text.
6. WHEN the user chooses an annotation color THEN Firaw SHALL show a preset visual palette whose initially selected primary color is Firaw cyan.

**Independent Test:** Draw a rectangle, undo, redo, copy, and paste into another application.

### P1: Choose a capture source

**User Story:** As a user, I want to capture a free region, an open window, or an entire monitor so that the capture matches what I intend to reuse.

**Acceptance Criteria:**

1. WHEN the launcher is visible THEN Firaw SHALL offer Region, Window, and Monitor capture actions.
2. WHEN the user chooses Window THEN Firaw SHALL show an in-app list of available program windows with their names and dimensions.
3. WHEN the user chooses Monitor THEN Firaw SHALL show an in-app list of every available monitor, identify the primary monitor, and show dimensions.
4. WHEN a monitor is confirmed THEN Firaw SHALL capture that complete monitor.
5. WHEN a window is confirmed THEN Firaw SHALL request pixels from that window so content is not replaced by another window positioned in front of it.
6. WHEN the user chooses Region THEN Firaw SHALL retain the adjustable Lightshot-style overlay.

**Independent Test:** Capture one target through each mode, including a partially covered window, and verify the resulting editor image.

### P1: Configure capture shortcuts

**User Story:** As a user, I want to choose the default capture mode and record my own shortcut so that Firaw fits my workflow.

**Acceptance Criteria:**

1. WHEN preferences open THEN Firaw SHALL let the user choose the default Region, Window, or Monitor mode used by global shortcuts.
2. WHEN the shortcut field has focus and the user presses a valid combination THEN Firaw SHALL display, save, and register that combination.
3. WHEN a shortcut is incomplete, reserved, or occupied THEN Firaw SHALL keep capture buttons usable and report the conflict.
4. WHEN Use Print Screen is selected THEN Firaw SHALL attempt to register Print Screen globally.
5. WHEN Windows owns Print Screen THEN Firaw SHALL provide a direct path to Keyboard settings and explain which Windows option to disable.
6. WHEN Firaw restarts THEN it SHALL restore saved capture preferences from the current user's local settings.
7. WHEN Start with Windows is selected THEN Firaw SHALL register the current executable for the current Windows user without requiring administrator access and start hidden in the tray at sign-in.

**Independent Test:** Save Ctrl+Alt+F9 with Monitor as default, restart Firaw, and verify both values and the shortcut behavior.

### P1: Copy text with local OCR

**User Story:** As a user, I want text from the captured area recognized locally so that I can copy it without retyping or uploading the image.

**Acceptance Criteria:**

1. WHEN the user chooses Copy text THEN Firaw SHALL recognize the original crop and copy all recognized text directly.
2. WHEN the user chooses Select text THEN Firaw SHALL activate a cyan region selector directly over the captured image.
3. WHEN the user releases a valid text region THEN Firaw SHALL recognize and copy only that image area without opening another window.
4. WHEN the user presses Escape while selecting text THEN Firaw SHALL cancel the mode without changing the image.
5. WHEN OCR finds no text or fails THEN Firaw SHALL show a clear non-destructive message.

**Independent Test:** Capture several lines, use Select text to drag over one line, and verify only that line is copied without another window opening.

### P1: Stay available in the system tray

**User Story:** As a user, I want Firaw to remain ready in the Windows notification area so that capture shortcuts keep working without a window occupying the taskbar.

**Acceptance Criteria:**

1. WHEN the user closes or minimizes the launcher THEN Firaw SHALL hide it and remain active in the system tray.
2. WHEN the user selects Open Firaw from the Eye of Horus menu THEN Firaw SHALL restore and activate the launcher.
3. WHEN the user opens the tray menu THEN Firaw SHALL offer Open, New capture, and Exit actions.
4. WHEN the launcher is hidden THEN global capture shortcuts SHALL remain active.
5. WHEN the Eye of Horus is shown in the fixed Windows tray slot THEN its artwork SHALL minimize transparent padding to remain clearly visible.
6. WHEN the user left-clicks the tray eye THEN Firaw SHALL start Region capture; Ctrl+left-click SHALL open the Window picker; middle-click SHALL open the Monitor picker; right-click SHALL preserve the full menu.

**Independent Test:** Close the launcher, verify the process and tray icon remain, invoke capture, then exit through the tray menu.

### P1: Reuse several copied texts

**User Story:** As a user, I want a collapsible list of copied texts so that I can select and combine several excerpts.

**Acceptance Criteria:**

1. WHEN text enters the Windows clipboard while Firaw is active THEN Firaw SHALL add it to an in-memory history with newest items first.
2. WHEN the user opens Texts THEN the editor SHALL show a right-side drawer without covering the image.
3. WHEN the user selects multiple entries with Ctrl or Shift and chooses Copy selected THEN Firaw SHALL combine them with blank lines.
4. WHEN the user chooses Import current THEN Firaw SHALL include the current text clipboard content.
5. WHEN the user chooses Clear list THEN Firaw SHALL remove the in-memory entries without changing files on disk.

**Independent Test:** Copy three texts in another app, open the drawer, select two, and verify the combined clipboard output.

### P1: Firaw visual identity

**User Story:** As a user, I want a recognizable Firaw identity so that the application and tray icon are easy to locate.

**Acceptance Criteria:**

1. WHEN Windows displays the executable, window, or tray entry THEN Firaw SHALL use the cyan Eye of Horus icon.
2. WHEN Windows 11 supports caption-color attributes THEN standard Firaw title bars SHALL use cyan instead of white with dark caption text.
3. WHEN the launcher is shown THEN its former white client background SHALL use Firaw cyan with dark high-contrast controls.

**Independent Test:** Inspect the executable icon, notification area, launcher, editor, and dialog title bars.

## Edge Cases

- WHEN capture is already active THEN Firaw SHALL ignore a duplicate invocation.
- WHEN the selection is smaller than two pixels THEN Firaw SHALL cancel it.
- WHEN OCR language data is unavailable THEN Firaw SHALL explain which local files are missing.
- WHEN the global Print Screen shortcut is occupied THEN Firaw SHALL keep the capture button and fallback shortcut available.

## Requirement Traceability

| Requirement ID | Story | Status |
| --- | --- | --- |
| CAP-01 | Select frozen region | Implemented; UAT pending |
| CAP-02 | Cancel safely | Implemented; UAT pending |
| CAP-03 | Hide all Firaw windows before freezing desktop | Implemented; UAT pending |
| CAP-04 | Move and resize active selection | Implemented; geometry tests passed, UAT pending |
| CAP-05 | Prevent DWM hide-transition ghosts in the snapshot | Implemented; UAT pending |
| CAP-06 | Region, window, and monitor capture modes | Implemented; UAT pending |
| CAP-07 | In-app target picker for windows and monitors | Implemented; visual smoke test passed |
| CAP-08 | Capture obscured window content through its window handle | Implemented; UAT pending |
| EDIT-01 | Draw annotations | Implemented; UAT pending |
| EDIT-02 | Undo and redo | Implemented; automated test passed |
| EDIT-03 | Add boxed notes to the image | Implemented; UAT pending |
| EDIT-04 | Preset visual color palette with Firaw cyan selected | Implemented; visual smoke test passed |
| IMG-01 | Copy rendered image | Implemented; UAT pending |
| IMG-02 | Save PNG | Implemented; UAT pending |
| OCR-01 | Local Portuguese/English recognition | Verified by automated smoke test |
| OCR-02 | Direct copy of all recognized text | Implemented; UAT pending |
| OCR-03 | Select an image region and copy its OCR without a dialog | Implemented; UAT pending |
| HOTKEY-01 | Global shortcut with fallback | Implemented; UAT pending |
| HOTKEY-02 | Persistent custom shortcut and default capture mode | Implemented; automated tests passed |
| HOTKEY-03 | Optional Print Screen registration and Windows settings path | Implemented; UAT pending |
| START-01 | Optional per-user startup with Windows | Implemented; automated path test passed |
| TRAY-01 | Continue running when launcher closes or minimizes | Implemented; UAT pending |
| TRAY-02 | Tray open, capture, and exit commands | Implemented; UAT pending |
| TRAY-03 | Tray mouse gestures for region, window, and monitor capture | Implemented; UAT pending |
| HIST-01 | Monitor text clipboard changes in memory | Implemented; automated service tests passed |
| HIST-02 | Collapsible multi-select text drawer | Implemented; UAT pending |
| BRAND-01 | Eye of Horus icon for executable, app, and tray | Implemented; UAT pending |
| BRAND-02 | Cyan Windows title bars where supported | Implemented; UAT pending |
| BRAND-03 | Cyan launcher background with dark contrast | Implemented; visual smoke test passed |
| BRAND-04 | Enlarged Eye of Horus artwork within the fixed tray slot | Implemented; asset inspected |

**Coverage:** 30 total, 30 mapped to tasks, 0 unmapped.

## Success Criteria

- [ ] The solution builds with no errors.
- [ ] Automated tests cover crop normalization and annotation history behavior.
- [ ] A user can complete capture-to-image-copy and capture-to-text-copy locally.
