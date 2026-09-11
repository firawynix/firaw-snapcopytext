using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class TextHistoryServiceTests
{
    [Fact]
    public void Add_PutsNewestTextFirstAndMovesDuplicates()
    {
        var history = new TextHistoryService();

        history.Add("primeiro", "Teste");
        history.Add("segundo", "Teste");
        history.Add("primeiro", "Teste");

        Assert.Equal(2, history.Items.Count);
        Assert.Equal("primeiro", history.Items[0].Text);
        Assert.Equal("segundo", history.Items[1].Text);
    }

    [Fact]
    public void Add_RespectsMaximumItemCount()
    {
        var history = new TextHistoryService(maximumItems: 2);

        history.Add("um", "Teste");
        history.Add("dois", "Teste");
        history.Add("três", "Teste");

        Assert.Equal(["três", "dois"], history.Items.Select(item => item.Text));
    }

    [Fact]
    public void Combine_JoinsSelectedTextsWithBlankLines()
    {
        var history = new TextHistoryService();
        var first = history.Add("linha um", "Teste")!;
        var second = history.Add("linha dois", "Teste")!;

        string result = TextHistoryService.Combine([first, second]);

        Assert.Equal($"linha um{Environment.NewLine}{Environment.NewLine}linha dois", result);
    }
}
