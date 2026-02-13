using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Entities.Auditing;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="CreationAuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="DefaultIdType"/>).
/// </summary>
[Serializable]
public abstract class CreationAuditedEntity : CreationAuditedEntity<DefaultIdType>, IEntity;

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAudited"/>.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class CreationAuditedEntity<TPrimaryKey> : Entity<TPrimaryKey>, ICreationAudited
{
    /// <summary>
    /// Creation time of this entity.
    /// </summary>
    public virtual DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creator of this entity.
    /// </summary>
    public virtual DefaultIdType CreatorUserId { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    protected CreationAuditedEntity()
    {
        // CreationTime = Clock.Now;
        CreatedAt = DateTime.Now;
    }
}

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAudited{TUser}"/>.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class CreationAuditedEntity<TPrimaryKey, TUser> : CreationAuditedEntity<TPrimaryKey>, ICreationAudited<TUser>
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    [ForeignKey("CreatorUserId")]
    public virtual TUser? CreatorUser { get; set; }
}