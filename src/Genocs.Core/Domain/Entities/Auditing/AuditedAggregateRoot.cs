using System.ComponentModel.DataAnnotations.Schema;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="AuditedAggregateRoot{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
/// </summary>
[Serializable]
public abstract class AuditedAggregateRoot : AuditedAggregateRoot<int>
{

}

/// <summary>
/// This class can be used to simplify implementing <see cref="IAudited"/> for aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity</typeparam>
[Serializable]
public abstract class AuditedAggregateRoot<TPrimaryKey> : CreationAuditedAggregateRoot<TPrimaryKey>, IAudited
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
/// This class can be used to simplify implementing <see cref="IAudited{TUser}"/> for aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class AuditedAggregateRoot<TPrimaryKey, TUser> : AuditedAggregateRoot<TPrimaryKey>, IAudited<TUser>
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