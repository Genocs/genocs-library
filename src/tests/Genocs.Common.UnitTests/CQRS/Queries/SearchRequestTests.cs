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

    [Fact]
    public void QAlias_ShouldReflectSearchTermValue()
    {
        SearchRequest sut = new()
        {
            SearchTerm = "books"
        };

        Assert.Equal("books", sut.q);
    }

    [Fact]
    public void SettingQAlias_ShouldUpdateSearchTerm()
    {
        SearchRequest sut = new();

        sut.q = "authors";

        Assert.Equal("authors", sut.SearchTerm);
    }

    [Fact]
    public void InterfaceSearchTerm_DefaultImplementation_ShouldMapToLegacyAlias()
    {
        ISearchRequest sut = new LegacySearchRequest();

        sut.SearchTerm = "publishers";

        Assert.Equal("publishers", sut.q);
    }

    private sealed class LegacySearchRequest : ISearchRequest
    {
        public string q { get; set; } = string.Empty;

        public int MaxItems { get; set; }
    }
}
