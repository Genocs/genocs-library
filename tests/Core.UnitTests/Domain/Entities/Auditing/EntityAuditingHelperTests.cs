using Genocs.Common.Domain.Entities.Auditing;
using Genocs.Core.Domain.Entities.Auditing;
using Xunit;

namespace Genocs.Core.UnitTests.Domain.Entities.Auditing;

public class EntityAuditingHelperTests
{
    [Fact]
    public void SetCreationAuditProperties_SetsCreatedAtAndCreator_WhenMissingAndUserKnown()
    {
        var entity = new TestAuditedObject();
        var userId = Guid.NewGuid();

        EntityAuditingHelper.SetCreationAuditProperties(entity, tenantId: null, userId: userId);

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAt.Kind);
        Assert.Equal(userId, entity.CreatorUserId);
    }

    [Fact]
    public void SetCreationAuditProperties_DoesNotOverrideCreatedAt_WhenAlreadySet()
    {
        DateTime expected = new DateTime(2026, 4, 4, 8, 0, 0, DateTimeKind.Utc);
        var entity = new TestAuditedObject { CreatedAt = expected };

        EntityAuditingHelper.SetCreationAuditProperties(entity, tenantId: null, userId: Guid.NewGuid());

        Assert.Equal(expected, entity.CreatedAt);
    }

    [Fact]
    public void SetCreationAuditProperties_DoesNotSetCreator_WhenUserUnknown()
    {
        var entity = new TestAuditedObject();

        EntityAuditingHelper.SetCreationAuditProperties(entity, tenantId: null, userId: null);

        Assert.Equal(default, entity.CreatorUserId);
    }

    [Fact]
    public void SetModificationAuditProperties_SetsUtcLastUpdate_AndUpdatedBy_WhenUserKnown()
    {
        var entity = new TestAuditedObject();
        var userId = Guid.NewGuid();

        EntityAuditingHelper.SetModificationAuditProperties(entity, tenantId: null, userId: userId);

        Assert.True(entity.LastUpdate.HasValue);
        Assert.Equal(DateTimeKind.Utc, entity.LastUpdate.Value.Kind);
        Assert.Equal(userId, entity.UpdatedBy);
    }

    [Fact]
    public void SetModificationAuditProperties_SetsUpdatedByNull_WhenUserUnknown()
    {
        var entity = new TestAuditedObject { UpdatedBy = Guid.NewGuid() };

        EntityAuditingHelper.SetModificationAuditProperties(entity, tenantId: null, userId: null);

        Assert.Null(entity.UpdatedBy);
    }

    private sealed class TestAuditedObject : ICreationAudited, IModificationAudited
    {
        public DateTime CreatedAt { get; set; }

        public DefaultIdType CreatorUserId { get; set; }

        public DateTime? LastUpdate { get; set; }

        public DefaultIdType? UpdatedBy { get; set; }
    }
}
