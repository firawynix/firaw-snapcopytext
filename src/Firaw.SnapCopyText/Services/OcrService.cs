using System.IO;
using System.Windows.Media.Imaging;
using Firaw.SnapCopyText.Models;
using Tesseract;

namespace Firaw.SnapCopyText.Services;

public sealed class OcrService
{
    private readonly string _dataPath;

    public OcrService(string? dataPath = null)
    {
        _dataPath = dataPath ?? Path.Combine(AppContext.BaseDirectory, "tessdata");
    }

    public Task<string> RecognizeAsync(BitmapSource image, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        byte[] png = EncodePng(image);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureLanguageData();

            using var engine = new TesseractEngine(_dataPath, "por+eng", EngineMode.LstmOnly);
            using Pix pix = Pix.LoadFromMemory(png);
            using Page page = engine.Process(pix, PageSegMode.Auto);
            cancellationToken.ThrowIfCancellationRequested();
            return page.GetText().Trim();
        }, cancellationToken);
    }

    public Task<IReadOnlyList<OcrTextRegion>> RecognizeTextRegionsAsync(
        BitmapSource image,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        byte[] png = EncodePng(image);

        return Task.Run<IReadOnlyList<OcrTextRegion>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureLanguageData();

            using var engine = new TesseractEngine(_dataPath, "por+eng", EngineMode.LstmOnly);
            using Pix pix = Pix.LoadFromMemory(png);
            using Page page = engine.Process(pix, PageSegMode.Auto);
            using ResultIterator iterator = page.GetIterator();
            var regions = new List<OcrTextRegion>();
            iterator.Begin();

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                string text = iterator.GetText(PageIteratorLevel.TextLine)?.Trim() ?? string.Empty;
                if (text.Length > 0 &&
                    iterator.TryGetBoundingBox(PageIteratorLevel.TextLine, out Tesseract.Rect bounds) &&
                    bounds.Width > 0 && bounds.Height > 0)
                {
                    regions.Add(new OcrTextRegion(
                        text,
                        new System.Windows.Int32Rect(bounds.X1, bounds.Y1, bounds.Width, bounds.Height)));
                }
            }
            while (iterator.Next(PageIteratorLevel.TextLine));

            return regions;
        }, cancellationToken);
    }

    private void EnsureLanguageData()
    {
        string[] missing = new[] { "por.traineddata", "eng.traineddata" }
            .Where(name => !File.Exists(Path.Combine(_dataPath, name)))
            .ToArray();

        if (missing.Length > 0)
        {
            throw new FileNotFoundException(
                $"Dados locais do OCR ausentes: {string.Join(", ", missing)}. Reinstale o Firaw - SnapCopyText.");
        }
    }

    private static byte[] EncodePng(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
