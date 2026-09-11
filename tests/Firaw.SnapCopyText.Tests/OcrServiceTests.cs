using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class OcrServiceTests
{
    [Fact]
    public async Task RecognizeAsync_ReadsClearEnglishTextLocally()
    {
        BitmapSource source = CreateTextImage("FIRAW 123");
        var service = new OcrService(Path.Combine(AppContext.BaseDirectory, "tessdata"));

        string result = await service.RecognizeAsync(source);

        Assert.Contains("FIRAW", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("123", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecognizeTextRegionsAsync_ReturnsTextAndImageBounds()
    {
        BitmapSource source = CreateTextImage("FIRAW 123");
        var service = new OcrService(Path.Combine(AppContext.BaseDirectory, "tessdata"));

        var regions = await service.RecognizeTextRegionsAsync(source);

        var region = Assert.Single(regions, item => item.Text.Contains("FIRAW", StringComparison.OrdinalIgnoreCase));
        Assert.True(region.Bounds.Width > 0);
        Assert.True(region.Bounds.Height > 0);
        Assert.InRange(region.Bounds.X, 0, source.PixelWidth - 1);
        Assert.InRange(region.Bounds.Y, 0, source.PixelHeight - 1);
    }

    private static BitmapSource CreateTextImage(string text)
    {
        using var bitmap = new Bitmap(640, 180, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        using var font = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Pixel);
        graphics.DrawString(text, font, Brushes.Black, new PointF(15, 35));

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
