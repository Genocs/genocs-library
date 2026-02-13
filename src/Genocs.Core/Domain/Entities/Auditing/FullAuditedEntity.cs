using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.Domain.Entities;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="FullAuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="Guid"/>).
/// </summary>
[Serializable]
public abstract class FullAuditedEntity : FullAuditedEntity<Guid>, IEntity
{

}

/// <summary>
/// Implements <see cref="IFullAudited"/> to be a base class for full-audited entities.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class FullAuditedEntity<TPrimaryKey> : AuditedEntity<TPrimaryKey>, IFullAudited
{
    /// <summary>
    /// it determines if the entity is deleted.
    /// Used for soft delete.
    /// </summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// It determines the user who deleted this entity.
    /// </summary>
    public virtual DefaultIdType? DeletedBy { get; set; }

    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    public virtual DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Implements <see cref="IFullAudited{TUser}"/> to be a base class for full-audited entities.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class FullAuditedEntity<TPrimaryKey, TUser> : AuditedEntity<TPrimaryKey, TUser>, IFullAudited<TUser>
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// It determines if the entity is deleted.
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