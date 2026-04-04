using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class PagedQueryBaseTests
{
    [Fact]
    public void Page_ShouldDefaultToZero()
    {
        TestPagedQuery sut = new();

        Assert.Equal(0, sut.Page);
    }

    [Fact]
    public void Results_ShouldDefaultToTen()
    {
        TestPagedQuery sut = new();

        Assert.Equal(10, sut.Results);
    }

    [Fact]
    public void OrderBy_ShouldDefaultToNull()
    {
        TestPagedQuery sut = new();

        Assert.Null(sut.OrderBy);
    }

    [Fact]
    public void SortOrder_ShouldDefaultToNull()
    {
        TestPagedQuery sut = new();

        Assert.Null(sut.SortOrder);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Page_ShouldAllowNegativeValues_InContractLayer(int value)
    {
        TestPagedQuery sut = new();

        sut.Page = value;

        Assert.Equal(value, sut.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Results_ShouldAllowNonPositiveValues_InContractLayer(int value)
    {
        TestPagedQuery sut = new();

        sut.Results = value;

        Assert.Equal(value, sut.Results);
    }

    private sealed class TestPagedQuery : PagedQueryBase;
}
