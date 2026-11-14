using Genocs.Common.Dependency;

namespace Genocs.Common.Interfaces;

/// <summary>
/// It is used to serialize and deserialize objects.
/// Host implementations are discovered as <see cref="ITransientDependency"/>.
/// </summary>
public interface ISerializerService : ITransientDependency
{
    /// <summary>
    /// The method is used to serialize an object.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">Instance of object to serialize.</param>
    /// <returns>The serialized string representation of the object.</returns>
    string Serialize<T>(T obj);

    string Serialize<T>(T obj, Type type);

    T Deserialize<T>(string text);
}