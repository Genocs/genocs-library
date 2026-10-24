namespace Genocs.Common.Domain.Entities.Auditing;

/// <summary>
/// An entity can implement this interface if <see cref="LastUpdate"/> of this entity must be stored.
/// <see cref="LastUpdate"/> is automatically set when updating an <see cref="IEntity"/> instance.
/// </summary>
public interface IHasModificationTime
{
    /// <summary>
    /// The last modified time for this entity.
    /// </summary>
    DateTime? LastUpdate { get; set; }
}
