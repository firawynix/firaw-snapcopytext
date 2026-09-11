using System.Collections.ObjectModel;
using Firaw.SnapCopyText.Models;

namespace Firaw.SnapCopyText.Services;

public sealed class TextHistoryService
{
    public const int DefaultMaximumItems = 100;
    public static TextHistoryService Shared { get; } = new();

    private readonly int _maximumItems;

    public ObservableCollection<CopiedTextEntry> Items { get; } = [];

    public TextHistoryService(int maximumItems = DefaultMaximumItems)
    {
        if (maximumItems < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumItems));
        }

        _maximumItems = maximumItems;
    }

    public CopiedTextEntry? Add(string? text, string source)
    {
        string normalized = text?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            return null;
        }

        CopiedTextEntry? duplicate = Items.FirstOrDefault(item => item.Text == normalized);
        if (duplicate is not null)
        {
            Items.Remove(duplicate);
        }

        var entry = new CopiedTextEntry(Guid.NewGuid(), normalized, source, DateTimeOffset.Now);
        Items.Insert(0, entry);

        while (Items.Count > _maximumItems)
        {
            Items.RemoveAt(Items.Count - 1);
        }

        return entry;
    }

    public static string Combine(IEnumerable<CopiedTextEntry> entries) =>
        string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            entries.Select(entry => entry.Text));
}
