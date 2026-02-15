using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genocs.HTTP;

/// <summary>
/// This class provides an implementation of the IHttpClientSerializer interface using
/// System.Text.Json for JSON serialization and deserialization. It allows for customizable JsonSerializerOptions to be provided,
/// or it will use default options that are suitable for most scenarios.
/// This serializer is designed to be used with the Genocs HTTP client for handling JSON content in HTTP requests and responses.
/// </summary>
public class SystemTextJsonHttpClientSerializer : IHttpClientSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonHttpClientSerializer(JsonSerializerOptions? options = null)
    {
        // Default options
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    public string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, _options);

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
}