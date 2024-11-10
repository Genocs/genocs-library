using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genocs.HTTP;

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

    public ValueTask<T?> DeserializeAsync<T>(Stream stream)
        => JsonSerializer.DeserializeAsync<T>(stream, _options);
}