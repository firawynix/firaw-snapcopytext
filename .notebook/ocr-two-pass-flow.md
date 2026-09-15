# OCR Two-pass Flow
> Small captures get an additional enlarged OCR pass without changing output coordinates

Entry: `src/Firaw.SnapCopyText/Services/OcrService.cs`

Flow:
- Encode the original `BitmapSource` once and recognize it with Tesseract `por+eng`.
- Compute an enlargement from the shortest image side, capped at 2.5x and eight million pixels.
- Run the enlarged image through the same engine and compare mean confidence and normalized text coverage.
- Keep the original result unless the enlarged pass has clearly better confidence or comparable confidence with meaningfully more text.
- For text-region OCR, divide enlarged bounding boxes by the scale and clamp them to the original image bounds before returning them.

Gotchas:
- Region coordinates must always remain in original-image pixels because automatic blur consumes them directly.
- Keep the enlargement bounded; full-monitor images should not receive an expensive second pass.
- Cancellation is checked before and during iterator traversal so an editor closing mid-OCR does not keep unnecessary work alive.

Verification:
- Unit tests cover normal OCR, missing language data, region bounds, and a small-text image that exercises enlargement.

Updated: 2026-09-15
