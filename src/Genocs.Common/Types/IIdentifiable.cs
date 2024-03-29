namespace Genocs.Common.Types;

/// <summary>
/// Identifiable interface definition.
/// </summary>
/// <typeparam name="T">The Identifiable type.</typeparam>
public interface IIdentifiable<out T>
{
    /// <summary>
    /// The Id getter.
    /// </summary>
    T Id { get; }

    /// <summary>
    /// Checks if this entity is transient (not persisted to database and it has not an <see cref="Id"/>).
    /// </summary>
    /// <returns>True, if this entity is transient, otherwise false.</returns>
    bool IsTransient();
}