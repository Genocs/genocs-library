namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// This interface is implemented by entities that is wanted to store modification information (who and when modified lastly).
/// Properties are automatically set when updating the <see cref="IEntity"/>.
/// </summary>
public interface IModificationAudited : IHasModificationTime
{
    /// <summary>
    /// Last modifier user for this entity.
    /// </summary>
    DefaultIdType? UpdatedBy { get; set; }
}

/// <summary>
/// Adds navigation properties to <see cref="IModificationAudited"/> interface for user.
/// </summary>
/// <typeparam name="TUser">Type of the user.</typeparam>
public interface IModificationAudited<TUser> : IModificationAudited
    where TUser : IEntity<DefaultIdType>
{
    /// <summary>
    /// Reference to the last modifier user of this entity.
    /// </summary>
    TUser? UpdatedByUser { get; set; }
}