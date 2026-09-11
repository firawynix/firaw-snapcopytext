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

**Independent Test:** Draw a rectangle, undo, redo, copy, and paste into another application.

### P1: Copy text with local OCR

**User Story:** As a user, I want text from the captured area recognized locally so that I can copy it without retyping or uploading the image.

**Acceptance Criteria:**

1. WHEN the user chooses Copy text THEN Firaw SHALL recognize the original crop and copy all recognized text directly.
2. WHEN the user chooses Select text THEN Firaw SHALL activate a cyan region selector directly over the captured image.
3. WHEN the user releases a valid text region THEN Firaw SHALL recognize and copy only that image area without opening another window.
4. WHEN the user presses Escape while selecting text THEN Firaw SHALL cancel the mode without changing the image.
5. WHEN OCR finds no text or fails THEN Firaw SHALL show a clear non-destructive message.

**Independent Test:** Capture several lines, use Select text to drag over one line, and verify only that line is copied without another window opening.

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
| EDIT-01 | Draw annotations | Implemented; UAT pending |
| EDIT-02 | Undo and redo | Implemented; automated test passed |
| EDIT-03 | Add boxed notes to the image | Implemented; UAT pending |
| IMG-01 | Copy rendered image | Implemented; UAT pending |
| IMG-02 | Save PNG | Implemented; UAT pending |
| OCR-01 | Local Portuguese/English recognition | Verified by automated smoke test |
| OCR-02 | Direct copy of all recognized text | Implemented; UAT pending |
| OCR-03 | Select an image region and copy its OCR without a dialog | Implemented; UAT pending |
| HOTKEY-01 | Global shortcut with fallback | Implemented; UAT pending |

**Coverage:** 14 total, 14 mapped to tasks, 0 unmapped.

## Success Criteria

- [ ] The solution builds with no errors.
- [ ] Automated tests cover crop normalization and annotation history behavior.
- [ ] A user can complete capture-to-image-copy and capture-to-text-copy locally.
