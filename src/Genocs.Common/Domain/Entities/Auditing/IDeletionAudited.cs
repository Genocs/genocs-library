using Genocs.Common.Domain.Entities;

namespace Genocs.Common.Domain.Entities.Auditing;

/// <summary>
/// This interface is implemented by entities which wanted to store deletion information (who and when deleted).
/// </summary>
public interface IDeletionAudited : IHasDeletionTime
{
    /// <summary>
    /// Which user deleted this entity.
    /// </summary>
    DefaultIdType? DeletedBy { get; set; }
}

/// <summary>
/// Adds navigation properties to <see cref="IDeletionAudited"/> interface for user.
/// </summary>
/// <typeparam name="TUser">Type of the user.</typeparam>
public interface IDeletionAudited<TUser> : IDeletionAudited
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// Reference to the deleter user of this entity.
    /// </summary>
    TUser? DeletedByUser { get; set; }
}
