namespace Genocs.HTTP;

/// <summary>
/// Defines methods for serializing objects to a string and deserializing objects from a stream, enabling conversion
/// between object instances and their textual representations for HTTP communication.
/// </summary>
/// <remarks>Implementations should ensure that serialized data can be accurately reconstructed during
/// deserialization. Consider performance and thread safety when handling large or complex objects. This interface is
/// typically used to customize how HTTP request and response bodies are processed in client applications.</remarks>
public interface IHttpClientSerializer
{
    /// <summary>
    /// Serializes the specified object to a string representation suitable for storage or transmission.
    /// </summary>
    /// <remarks>Use this method to convert complex objects into a format that can be persisted or sent over a
    /// network. The object must be serializable; otherwise, serialization may fail.</remarks>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize. This parameter cannot be null.</param>
    /// <returns>A string containing the serialized form of the specified object.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the content of the specified stream into an object of type T asynchronously.
    /// </summary>
    /// <remarks>Ensure that the stream contains data formatted for the expected type T. This method is
    /// asynchronous and should be awaited. The stream is not closed or disposed by this method.</remarks>
    /// <typeparam name="T">The type of the object to deserialize from the stream.</typeparam>
    /// <param name="stream">The stream containing the data to deserialize. The stream must be readable and positioned at the beginning of
    /// the data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The result contains the deserialized object of type T,
    /// or null if the stream is empty.</returns>
    ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);
}