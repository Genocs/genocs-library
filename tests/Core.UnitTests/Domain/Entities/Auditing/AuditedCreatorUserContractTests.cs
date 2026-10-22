using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Entities.Auditing;
using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Entities.Auditing;
using Xunit;

namespace Genocs.Core.UnitTests.Domain.Entities.Auditing;

public class AuditedCreatorUserContractTests
{
    [Fact]
    public void CreationAuditedEntity_ImplementsCreatorUserContract()
    {
        var entity = new TestCreationAuditedEntity();
        ICreationAudited<TestUser> audited = entity;
        var user = new TestUser { Id = Guid.NewGuid() };

        audited.CreatorUser = user;

        Assert.Same(user, audited.CreatorUser);
        Assert.Same(user, entity.CreatorUser);
    }

    [Fact]
    public void AuditedEntity_ImplementsCreatorUserContract()
    {
        var entity = new TestAuditedEntity();
        IAudited<TestUser> audited = entity;
        var user = new TestUser { Id = Guid.NewGuid() };

        audited.CreatorUser = user;

        Assert.Same(user, audited.CreatorUser);
        Assert.Same(user, entity.CreatorUser);
    }

    [Fact]
    public void CreationAuditedAggregateRoot_ImplementsCreatorUserContract()
    {
        var entity = new TestCreationAuditedAggregateRoot();
        ICreationAudited<TestUser> audited = entity;
        var user = new TestUser { Id = Guid.NewGuid() };

        audited.CreatorUser = user;

        Assert.Same(user, audited.CreatorUser);
        Assert.Same(user, entity.CreatorUser);
    }

    [Fact]
    public void AuditedAggregateRoot_ImplementsCreatorUserContract()
    {
        var entity = new TestAuditedAggregateRoot();
        IAudited<TestUser> audited = entity;
        var user = new TestUser { Id = Guid.NewGuid() };

        audited.CreatorUser = user;

        Assert.Same(user, audited.CreatorUser);
        Assert.Same(user, entity.CreatorUser);
    }

    private sealed class TestUser : Entity<DefaultIdType>;

    private sealed class TestCreationAuditedEntity : CreationAuditedEntity<Guid, TestUser>;

    private sealed class TestAuditedEntity : AuditedEntity<Guid, TestUser>;

    private sealed class TestCreationAuditedAggregateRoot : CreationAuditedAggregateRoot<int, TestUser>;

    private sealed class TestAuditedAggregateRoot : AuditedAggregateRoot<int, TestUser>;
}
