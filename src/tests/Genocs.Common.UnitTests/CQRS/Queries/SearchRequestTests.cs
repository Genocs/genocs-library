using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class SearchRequestTests
{
    [Fact]
    public void SearchTerm_ShouldDefaultToEmptyString()
    {
        SearchRequest sut = new();

        Assert.Equal(string.Empty, sut.SearchTerm);
    }
}
