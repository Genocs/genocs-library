namespace Genocs.Core.Domain.Entities;

/// <summary>
/// The base interface for all entities.
/// </summary>
public interface IEntity
{

    /// <summary>
    /// Checks if this entity is transient (not persisted to database) />).
    /// </summary>
    /// <returns>True, if this entity is transient, otherwise false.</returns>
    bool IsTransient();
}