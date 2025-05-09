﻿using System.ComponentModel.DataAnnotations.Schema;

// using Genocs.Timing;

namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// A shortcut of <see cref="CreationAuditedAggregateRoot{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
/// </summary>
[Serializable]
public abstract class CreationAuditedAggregateRoot : CreationAuditedAggregateRoot<int>
{
}

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAudited"/> for aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class CreationAuditedAggregateRoot<TPrimaryKey> : AggregateRoot<TPrimaryKey>, ICreationAudited
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
    protected CreationAuditedAggregateRoot()
    {
        // CreationTime = Clock.Now; // Manage time zone
        CreatedAt = DateTime.Now;
    }
}

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAudited{TUser}"/> for aggregate roots.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
/// <typeparam name="TUser">Type of the user.</typeparam>
[Serializable]
public abstract class CreationAuditedAggregateRoot<TPrimaryKey, TUser> : CreationAuditedAggregateRoot<TPrimaryKey>, ICreationAudited<TUser>
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    [ForeignKey("CreatorUserId")]
    public virtual TUser? CreatorUser { get; set; }
}
