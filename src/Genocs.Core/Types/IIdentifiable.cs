namespace Genocs.Core.Types;

/// <summary>
/// Identifiable interface definition
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IIdentifiable<out T>
{
    /// <summary>
    /// The Id getter
    /// </summary>
    T Id { get; }
}