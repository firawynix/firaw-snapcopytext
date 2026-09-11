# Windows MVP Specification

## Problem Statement

Users need to capture a screen region, mark it up, and reuse either the image or its text immediately. Existing lightweight tools copy images well but do not make private, local text extraction a primary action.

## Goals

- [ ] Capture and open a selected region in the editor.
- [ ] Copy edited pixels or locally recognized text without network access.
- [ ] Provide the most useful annotation and redaction tools without slowing the capture flow.

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

1. WHEN the user invokes capture THEN Firaw SHALL display a frozen desktop overlay.
2. WHEN the user drags a non-empty rectangle THEN Firaw SHALL open that crop in the editor.
3. WHEN the user presses Escape or makes an empty selection THEN Firaw SHALL cancel safely.

**Independent Test:** Invoke capture, drag over visible content, and verify the editor contains that crop.

### P1: Copy or save the image

**User Story:** As a user, I want to annotate and copy or save the result so that I can use it in another application.

**Acceptance Criteria:**

1. WHEN the user draws an annotation THEN Firaw SHALL render it above the capture.
2. WHEN the user chooses Copy image THEN Firaw SHALL place the rendered image on the Windows clipboard.
3. WHEN the user chooses Save THEN Firaw SHALL write a PNG selected by the user.
4. WHEN the user chooses Undo or Redo THEN Firaw SHALL update the latest annotation accordingly.

**Independent Test:** Draw a rectangle, undo, redo, copy, and paste into another application.

### P1: Copy text with local OCR

**User Story:** As a user, I want text from the captured area recognized locally so that I can copy it without retyping or uploading the image.

**Acceptance Criteria:**

1. WHEN the user chooses Copy text THEN Firaw SHALL recognize the original crop using local Portuguese and English OCR data.
2. WHEN OCR finds text THEN Firaw SHALL show an editable preview and allow copying it.
3. WHEN OCR finds no text or fails THEN Firaw SHALL show a clear non-destructive message.

**Independent Test:** Capture a Portuguese sentence, run OCR, edit the preview, and copy the result.

## Edge Cases

- WHEN capture is already active THEN Firaw SHALL ignore a duplicate invocation.
- WHEN the selection is smaller than two pixels THEN Firaw SHALL cancel it.
- WHEN OCR language data is unavailable THEN Firaw SHALL explain which local files are missing.
- WHEN the global Print Screen shortcut is occupied THEN Firaw SHALL keep the capture button and fallback shortcut available.

## Requirement Traceability

| Requirement ID | Story | Status |
| --- | --- | --- |
| CAP-01 | Select frozen region | In Tasks |
| CAP-02 | Cancel safely | In Tasks |
| EDIT-01 | Draw annotations | In Tasks |
| EDIT-02 | Undo and redo | In Tasks |
| IMG-01 | Copy rendered image | In Tasks |
| IMG-02 | Save PNG | In Tasks |
| OCR-01 | Local Portuguese/English recognition | In Tasks |
| OCR-02 | Editable preview and copy | In Tasks |
| HOTKEY-01 | Global shortcut with fallback | In Tasks |

**Coverage:** 9 total, 9 mapped to tasks, 0 unmapped.

## Success Criteria

- [ ] The solution builds with no errors.
- [ ] Automated tests cover crop normalization and annotation history behavior.
- [ ] A user can complete capture-to-image-copy and capture-to-text-copy locally.
