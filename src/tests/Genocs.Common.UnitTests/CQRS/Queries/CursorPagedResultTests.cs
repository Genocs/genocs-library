using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class CursorPagedResultTests
{
    [Fact]
    public void Empty_ShouldReturnNoItemsAndNoCursors()
    {
        CursorPagedResult<int> result = CursorPagedResult<int>.Empty;

        Assert.Empty(result.Items);
        Assert.Equal(0, result.ReturnedCount);
        Assert.Null(result.NextCursor);
        Assert.Null(result.PreviousCursor);
        Assert.False(result.HasMore);
    }

    [Fact]
    public void Create_ShouldPopulateItemsAndCursorMetadata()
    {
        int[] items = [1, 2, 3];

        CursorPagedResult<int> result = CursorPagedResult<int>.Create(
            items,
            nextCursor: "next-token",
            previousCursor: "prev-token");

        Assert.Equal(items, result.Items);
        Assert.Equal(3, result.ReturnedCount);
        Assert.Equal("next-token", result.NextCursor);
        Assert.Equal("prev-token", result.PreviousCursor);
        Assert.True(result.HasMore);
    }

    [Fact]
    public void Create_WithNullItems_ShouldReturnEmptyCollection()
    {
        CursorPagedResult<int> result = CursorPagedResult<int>.Create(null!);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.ReturnedCount);
        Assert.False(result.HasMore);
    }

    [Fact]
    public void HasMore_ShouldBeFalse_WhenNextCursorIsWhitespace()
    {
        CursorPagedResult<int> result = CursorPagedResult<int>.Create([1], nextCursor: "   ");

        Assert.False(result.HasMore);
    }
}
