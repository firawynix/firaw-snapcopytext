using Firaw.SnapCopyText.Models;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class AnnotationHistoryTests
{
    [Fact]
    public void UndoAndRedo_ReturnLatestAnnotation()
    {
        var history = new AnnotationHistory<object>();
        var first = new object();
        var second = new object();
        history.Add(first);
        history.Add(second);

        Assert.Same(second, history.Undo());
        Assert.True(history.CanRedo);
        Assert.Same(second, history.Redo());
        Assert.Equal([first, second], history.Items);
    }

    [Fact]
    public void Add_AfterUndo_ClearsRedoBranch()
    {
        var history = new AnnotationHistory<object>();
        history.Add(new object());
        history.Undo();

        history.Add(new object());

        Assert.False(history.CanRedo);
        Assert.Single(history.Items);
    }

    [Fact]
    public void EmptyHistory_ReturnsNullForUndoAndRedo()
    {
        var history = new AnnotationHistory<object>();

        Assert.Null(history.Undo());
        Assert.Null(history.Redo());
    }
}
