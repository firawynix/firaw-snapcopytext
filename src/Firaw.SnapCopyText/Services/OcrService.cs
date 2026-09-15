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
        int width = image.PixelWidth;
        int height = image.PixelHeight;

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureLanguageData();

            using var engine = new TesseractEngine(_dataPath, "por+eng", EngineMode.LstmOnly);
            using Pix pix = Pix.LoadFromMemory(png);
            OcrPass original = ReadPass(engine, pix, 1, width, height, includeRegions: false, cancellationToken);
            float enhancementScale = DetermineEnhancementScale(width, height);
            if (enhancementScale <= 1.05f)
            {
                return original.Text;
            }

            using Pix enhancedPix = pix.Scale(enhancementScale, enhancementScale);
            OcrPass enhanced = ReadPass(
                engine,
                enhancedPix,
                enhancementScale,
                width,
                height,
                includeRegions: false,
                cancellationToken);
            return SelectBestPass(original, enhanced).Text;
        }, cancellationToken);
    }

    public Task<IReadOnlyList<OcrTextRegion>> RecognizeTextRegionsAsync(
        BitmapSource image,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        byte[] png = EncodePng(image);
        int width = image.PixelWidth;
        int height = image.PixelHeight;

        return Task.Run<IReadOnlyList<OcrTextRegion>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureLanguageData();

            using var engine = new TesseractEngine(_dataPath, "por+eng", EngineMode.LstmOnly);
            using Pix pix = Pix.LoadFromMemory(png);
            OcrPass original = ReadPass(engine, pix, 1, width, height, includeRegions: true, cancellationToken);
            float enhancementScale = DetermineEnhancementScale(width, height);
            if (enhancementScale <= 1.05f)
            {
                return original.Regions;
            }

            using Pix enhancedPix = pix.Scale(enhancementScale, enhancementScale);
            OcrPass enhanced = ReadPass(
                engine,
                enhancedPix,
                enhancementScale,
                width,
                height,
                includeRegions: true,
                cancellationToken);
            return SelectBestPass(original, enhanced).Regions;
        }, cancellationToken);
    }

    private static OcrPass ReadPass(
        TesseractEngine engine,
        Pix pix,
        float scale,
        int originalWidth,
        int originalHeight,
        bool includeRegions,
        CancellationToken cancellationToken)
    {
        using Page page = engine.Process(pix, PageSegMode.Auto);
        cancellationToken.ThrowIfCancellationRequested();
        string text = NormalizeText(page.GetText());
        float confidence = page.GetMeanConfidence();
        if (!includeRegions)
        {
            return new OcrPass(text, confidence, []);
        }

        using ResultIterator iterator = page.GetIterator();
        var regions = new List<OcrTextRegion>();
        iterator.Begin();
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            string line = NormalizeText(iterator.GetText(PageIteratorLevel.TextLine));
            if (line.Length > 0 &&
                iterator.TryGetBoundingBox(PageIteratorLevel.TextLine, out Tesseract.Rect bounds) &&
                bounds.Width > 0 && bounds.Height > 0)
            {
                regions.Add(new OcrTextRegion(
                    line,
                    MapBounds(bounds, scale, originalWidth, originalHeight)));
            }
        }
        while (iterator.Next(PageIteratorLevel.TextLine));

        return new OcrPass(text, confidence, regions);
    }

    private static OcrPass SelectBestPass(OcrPass original, OcrPass enhanced)
    {
        if (string.IsNullOrWhiteSpace(original.Text))
        {
            return enhanced;
        }
        if (string.IsNullOrWhiteSpace(enhanced.Text))
        {
            return original;
        }
        if (enhanced.Confidence > original.Confidence + 0.015f)
        {
            return enhanced;
        }
        if (enhanced.Confidence + 0.015f >= original.Confidence &&
            enhanced.Text.Length > original.Text.Length * 1.08)
        {
            return enhanced;
        }

        return original;
    }

    private static float DetermineEnhancementScale(int width, int height)
    {
        const double maximumEnhancedPixels = 8_000_000;
        double maximumScale = Math.Sqrt(maximumEnhancedPixels / Math.Max(1d, width * (double)height));
        double desiredScale = Math.Clamp(1200d / Math.Max(1, Math.Min(width, height)), 1.25, 2.5);
        return (float)Math.Max(1, Math.Min(desiredScale, maximumScale));
    }

    private static System.Windows.Int32Rect MapBounds(
        Tesseract.Rect bounds,
        float scale,
        int originalWidth,
        int originalHeight)
    {
        int left = Math.Clamp((int)Math.Floor(bounds.X1 / scale), 0, originalWidth - 1);
        int top = Math.Clamp((int)Math.Floor(bounds.Y1 / scale), 0, originalHeight - 1);
        int right = Math.Clamp((int)Math.Ceiling(bounds.X2 / scale), left + 1, originalWidth);
        int bottom = Math.Clamp((int)Math.Ceiling(bounds.Y2 / scale), top + 1, originalHeight);
        return new System.Windows.Int32Rect(left, top, right - left, bottom - top);
    }

    private static string NormalizeText(string? text) => string.Join(
        Environment.NewLine,
        (text ?? string.Empty)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.TrimEnd()))
        .Trim();

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

    private sealed record OcrPass(
        string Text,
        float Confidence,
        IReadOnlyList<OcrTextRegion> Regions);
}
