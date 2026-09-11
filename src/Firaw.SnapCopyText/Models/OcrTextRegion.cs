using System.Windows;

namespace Firaw.SnapCopyText.Models;

public sealed record OcrTextRegion(string Text, Int32Rect Bounds);
