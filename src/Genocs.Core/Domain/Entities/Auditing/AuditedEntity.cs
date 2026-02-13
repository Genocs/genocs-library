using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.Domain.Entities;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="AuditedEntity{TPrimaryKey}"/> for most used primary key type (<see cref="Guid"/>).
/// </summary>
[Serializable]
public abstract class AuditedEntity : AuditedEntity<DefaultIdType>, IEntity;

/// <summary>
/// This class can be used to simplify implementing <see cref="IAudited"/>.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class AuditedEntity<TPrimaryKey> : CreationAuditedEntity<TPrimaryKey>, IAudited
{
    /// <summary>
    /// Last modification date of this entity.
    /// </summary>
    public virtual DateTime? LastUpdate { get; set; }

    /// <summary>
    /// Last modifier user of this entity.
    /// </summary>
    public virtual DefaultIdType? UpdatedBy { get; set; }
}

/// <summary>
/// This class can be used to simplify implementing <see cref="IAudited{TUser}"/>.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class AuditedEntity<TPrimaryKey, TUser> : AuditedEntity<TPrimaryKey>, IAudited<TUser>
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    [ForeignKey("CreatorUserId")]
    public virtual TUser? CreatorUser { get; set; }

    /// <summary>
    /// Reference to the last modifier user of this entity.
    /// </summary>
    [ForeignKey("LastModifierUserId")]
    public virtual TUser? UpdatedByUser { get; set; }
}