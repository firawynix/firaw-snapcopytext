namespace Firaw.SnapCopyText.Models;

public sealed record CopiedTextEntry(
    Guid Id,
    string Text,
    string Source,
    DateTimeOffset CopiedAt)
{
    public string Preview
    {
        get
        {
            string singleLine = Text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            return singleLine.Length <= 150 ? singleLine : $"{singleLine[..147]}…";
        }
    }

    public string Details => $"{Source}  •  {CopiedAt:HH:mm}";
}
