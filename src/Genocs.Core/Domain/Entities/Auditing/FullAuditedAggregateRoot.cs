using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Entities.Auditing;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="FullAuditedAggregateRoot{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
/// </summary>
[Serializable]
public abstract class FullAuditedAggregateRoot : FullAuditedAggregateRoot<int>
{

}

/// <summary>
/// Implements <see cref="IFullAudited"/> to be a base class for full-audited aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class FullAuditedAggregateRoot<TPrimaryKey> : AuditedAggregateRoot<TPrimaryKey>, IFullAudited
{
    /// <summary>
    /// It defines whether this entity is deleted or not.
    /// </summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Which user deleted this entity.
    /// </summary>
    public virtual DefaultIdType? DeletedBy { get; set; }

    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    public virtual DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Implements <see cref="IFullAudited{TUser}"/> to be a base class for full-audited aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class FullAuditedAggregateRoot<TPrimaryKey, TUser> : AuditedAggregateRoot<TPrimaryKey, TUser>, IFullAudited<TUser>
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// It defines whether this entity is deleted or not.
    /// </summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Reference to the deleter user of this entity.
    /// </summary>
    [ForeignKey("DeleterUserId")]
    public virtual TUser? DeletedByUser { get; set; }

    /// <summary>
    /// Which user deleted this entity.
    /// </summary>
    public virtual DefaultIdType? DeletedBy { get; set; }

    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    public virtual DateTime? DeletedAt { get; set; }
}