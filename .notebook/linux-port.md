# Linux port

The native Linux edition is under `linux/`. Electron uses desktopCapturer for
screens/windows, global shortcuts and the native clipboard. OCR stays offline
through Tesseract.js and the repository's Portuguese/English trained data,
which are copied into the AppImage as resources.

Run `npm test` and `npm run build:linux` from `linux/`. The stable Center URL is
`https://jogos.firawynix.com.br/api/games/snapcopytext/linux/arquivo`.
