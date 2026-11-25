using Genocs.Persistence.CosmosDB.Domain.Repositories;
using Xunit;

namespace Genocs.Persistence.CosmosDB.UnitTests;

public class CosmosItemIdentityTests
{
    [Fact]
    public void ToItemId_FormatsGuidAsDashedString()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        string itemId = CosmosItemIdentity.ToItemId(id);

        Assert.Equal("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", itemId);
    }

    [Fact]
    public void ToItemId_ReturnsStringId()
    {
        Assert.Equal("order-1", CosmosItemIdentity.ToItemId("order-1"));
    }

    [Fact]
    public void ToItemId_RejectsEmptyValues()
    {
        Assert.Throws<ArgumentException>(() => CosmosItemIdentity.ToItemId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => CosmosItemIdentity.ToItemId("  "));
    }

    [Fact]
    public void EnsureSupportedKey_RejectsInt()
    {
        Assert.Throws<NotSupportedException>(CosmosItemIdentity.EnsureSupportedKey<int>);
    }
}
