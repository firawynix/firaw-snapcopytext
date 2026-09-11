namespace Firaw.SnapCopyText.Models;

public sealed class AnnotationHistory<T> where T : class
{
    private readonly List<T> _items = [];
    private readonly Stack<T> _redoItems = [];

    public IReadOnlyList<T> Items => _items;
    public bool CanUndo => _items.Count > 0;
    public bool CanRedo => _redoItems.Count > 0;

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
        _redoItems.Clear();
    }

    public T? Undo()
    {
        if (!CanUndo)
        {
            return null;
        }

        T item = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        _redoItems.Push(item);
        return item;
    }

    public T? Redo()
    {
        if (!CanRedo)
        {
            return null;
        }

        T item = _redoItems.Pop();
        _items.Add(item);
        return item;
    }

    public void Clear()
    {
        _items.Clear();
        _redoItems.Clear();
    }
}
