namespace Genocs.Common.Domain.Entities;

/// <summary>
/// The base interface for all entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Checks if this entity is new and has not been persisted yet.
    /// </summary>
    /// <returns>True when this entity has not been persisted yet; otherwise false.</returns>
    /// <remarks>
    /// This is the preferred lifecycle semantic because it is less likely to be confused with DI lifetime terminology.
    /// </remarks>
    bool IsNew() => IsTransient();

    /// <summary>
    /// Checks if this entity is transient (not persisted to database).
    /// </summary>
    /// <returns>True, if this entity is transient, otherwise False.</returns>
    /// <remarks>
    /// Kept for backward compatibility. Prefer <see cref="IsNew"/> in new code.
    /// </remarks>
    bool IsTransient();
}