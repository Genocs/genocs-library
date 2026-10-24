namespace Genocs.Common.Domain.Entities.Auditing;

/// <summary>
/// An entity can implement this interface if <see cref="CreatedAt"/> of this entity must be stored.
/// <see cref="CreatedAt"/> is automatically set when saving an <see cref="IEntity"/> instance.
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// Creation time of this entity.
    /// </summary>
    DateTime CreatedAt { get; }
}
