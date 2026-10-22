using Genocs.Common.CQRS.Queries;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Queries;

public class SoftDeleteFilterContractsTests
{
    [Fact]
    public void SoftDeleteFilterBase_ShouldDefaultToExcludeDeletedRecords()
    {
        TestSoftDeleteQuery query = new();

        Assert.False(query.IncludeDeleted);
    }

    [Fact]
    public void SoftDeleteFilterBase_ShouldAllowOptInForDeletedRecords()
    {
        TestSoftDeleteQuery query = new()
        {
            IncludeDeleted = true
        };

        Assert.True(query.IncludeDeleted);
    }

    [Fact]
    public void SoftDeleteFilterContract_ShouldExposeIncludeDeletedProperty()
    {
        var property = typeof(ISoftDeleteFilter).GetProperty(nameof(ISoftDeleteFilter.IncludeDeleted));

        Assert.NotNull(property);
        Assert.Equal(typeof(bool), property!.PropertyType);
    }

    private sealed class TestSoftDeleteQuery : SoftDeleteFilterBase;
}