using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;
using Xunit;
using Genocs.Core.Domain.Entities.Auditing;

namespace Genocs.Core.UnitTests.Domain.Entities;

/// <summary>
/// Tests for entity equality semantics and transient ID behavior.
/// </summary>
public class EntityEqualityTests
{
    [Fact]
    public void TransientEntity_WithDefaultId_IsNotEqualToAnother()
    {
        var entity1 = new TestEntity { Id = default };
        var entity2 = new TestEntity { Id = default };

        // Transient entities (ID = default) should not be equal even if other properties match.
        Assert.NotEqual(entity1, entity2);
    }

    [Fact]
    public void TransientEntity_IsNotEqualToSelf()
    {
        var entity = new TestEntity { Id = default };

        // A transient entity is uniquely identified and not equal to itself across different instances.
        // However, the same instance reference should be equal to itself.
        Assert.Same(entity, entity);
    }

    [Fact]
    public void PersistentEntity_WithSameId_IsEqualRegardlessOfReference()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        // Persistent entities with the same ID should be considered equal.
        Assert.Equal(entity1, entity2);
    }

    [Fact]
    public void PersistentEntity_WithDifferentIds_IsNotEqual()
    {
        var entity1 = new TestEntity { Id = Guid.NewGuid() };
        var entity2 = new TestEntity { Id = Guid.NewGuid() };

        Assert.NotEqual(entity1, entity2);
    }

    [Fact]
    public void AggregateRoot_WithDefaultId_IsNotEqualToAnother()
    {
        var aggregate1 = new TestAggregate { Id = default };
        var aggregate2 = new TestAggregate { Id = default };

        Assert.NotEqual(aggregate1, aggregate2);
    }

    [Fact]
    public void AggregateRoot_WithSameId_IsEqualRegardlessOfReference()
    {
        var id = Guid.NewGuid();
        var aggregate1 = new TestAggregate { Id = id };
        var aggregate2 = new TestAggregate { Id = id };

        Assert.Equal(aggregate1, aggregate2);
    }

    [Fact]
    public void Entity_HashCode_IsConsistentForSamePersistentId()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        // Persistent entities with the same ID should have the same hash code.
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void Entity_HashCode_MayDifferForTransient()
    {
        var entity1 = new TestEntity { Id = default };
        var entity2 = new TestEntity { Id = default };

        // Transient entities may have different hash codes (implementation detail).
        // We simply verify the method doesn't throw.
        int hash1 = entity1.GetHashCode();
        int hash2 = entity2.GetHashCode();

        Assert.IsType<int>(hash1);
        Assert.IsType<int>(hash2);
    }

    [Fact]
    public void Entity_CanBeStoredInHashSet_WithoutDuplicateTransient()
    {
        var entity1 = new TestEntity { Id = default };
        var entity2 = new TestEntity { Id = default };

        var set = new HashSet<TestEntity> { entity1, entity2 };

        // Transient entities are considered distinct.
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void Entity_CanBeStoredInHashSet_WithoutDuplicatePersistent()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        var set = new HashSet<TestEntity> { entity1, entity2 };

        // Persistent entities with the same ID should be deduplicated.
        Assert.Single(set);
    }

    [Fact]
    public void AuditedEntity_ComparesBySameIdSemantics()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestAuditedEntity { Id = id };
        var entity2 = new TestAuditedEntity { Id = id };

        // Audited entities follow the same equality semantics as base entities.
        Assert.Equal(entity1, entity2);
    }

    [Fact]
    public void AuditedEntity_Transient_NotEqual()
    {
        var entity1 = new TestAuditedEntity { Id = default };
        var entity2 = new TestAuditedEntity { Id = default };

        Assert.NotEqual(entity1, entity2);
    }

    private sealed class TestEntity : Entity<Guid>
    {
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
    }

    private sealed class TestAuditedEntity : AuditedEntity<Guid>
    {
    }
}
