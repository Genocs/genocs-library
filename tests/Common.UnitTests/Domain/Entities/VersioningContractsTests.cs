using Genocs.Common.Domain.Entities;
using Xunit;

namespace Genocs.Common.UnitTests.Domain.Entities;

public class VersioningContractsTests
{
    [Fact]
    public void VersionedContract_ShouldExposeLongVersionProperty()
    {
        var property = typeof(IVersioned).GetProperty(nameof(IVersioned.Version));

        Assert.NotNull(property);
        Assert.Equal(typeof(long), property!.PropertyType);
    }

    [Fact]
    public void EntityCanComposeWithVersionedContract()
    {
        var entity = new VersionedEntity
        {
            Id = Guid.NewGuid(),
            Version = 7
        };

        Assert.False(entity.IsTransient());
        Assert.Equal(7, entity.Version);
    }

    private sealed class VersionedEntity : IEntity<Guid>, IVersioned
    {
        public Guid Id { get; set; }

        public long Version { get; set; }

        public bool IsTransient() => Id == Guid.Empty;

        public bool IsNew() => IsTransient();
    }
}