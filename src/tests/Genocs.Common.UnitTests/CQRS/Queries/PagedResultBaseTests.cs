using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class PagedResultBaseTests
{
    [Fact]
    public void HasPreviousPage_ShouldBeFalse_OnFirstPage()
    {
        PagedResult<int> result = PagedResult<int>.Create([], currentPage: 0, resultsPerPage: 10, totalPages: 3, totalResults: 25);

        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrue_OnSecondPage()
    {
        PagedResult<int> result = PagedResult<int>.Create([], currentPage: 1, resultsPerPage: 10, totalPages: 3, totalResults: 25);

        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_ShouldRemainTrue_OnLaterPages()
    {
        PagedResult<int> result = PagedResult<int>.Create([], currentPage: 2, resultsPerPage: 10, totalPages: 3, totalResults: 25);

        Assert.True(result.HasPreviousPage);
    }

    [Theory]
    [InlineData(-1, 3)] // Negative page
    [InlineData(3, 3)] // Page index equal to totalPages (out of range)
    [InlineData(10, 3)] // Page index much greater than totalPages
    public void Constructor_ShouldThrow_OnOutOfRangePage(int currentPage, int totalPages)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PagedResult<int>.Create([], currentPage, 10, totalPages, 25));
    }
}