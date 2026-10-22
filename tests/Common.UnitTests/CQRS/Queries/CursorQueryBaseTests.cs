using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class CursorQueryBaseTests
{
    [Fact]
    public void Cursor_ShouldDefaultToNull()
    {
        TestCursorQuery sut = new();

        Assert.Null(sut.Cursor);
    }

    [Fact]
    public void Limit_ShouldDefaultToTen()
    {
        TestCursorQuery sut = new();

        Assert.Equal(10, sut.Limit);
    }

    [Fact]
    public void OrderBy_ShouldDefaultToNull()
    {
        TestCursorQuery sut = new();

        Assert.Null(sut.OrderBy);
    }

    [Fact]
    public void SortOrder_ShouldDefaultToNull()
    {
        TestCursorQuery sut = new();

        Assert.Null(sut.SortOrder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Limit_ShouldAllowNonPositiveValues_InContractLayer(int value)
    {
        TestCursorQuery sut = new();

        sut.Limit = value;

        Assert.Equal(value, sut.Limit);
    }

    [Theory]
    [InlineData("opaque-cursor-token")]
    [InlineData("eyJpZCI6MTIzfQ==")]
    public void Cursor_ShouldAllowOpaqueTokens(string value)
    {
        TestCursorQuery sut = new();

        sut.Cursor = value;

        Assert.Equal(value, sut.Cursor);
    }

    private sealed class TestCursorQuery : CursorQueryBase;
}
