using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

namespace Genocs.Persistence.CosmosDB.Serialization;

/// <summary>
/// Serializes documents with camelCase names so <c>Id</c> is stored as the required Cosmos <c>id</c>
/// and <c>PartitionKey</c> is stored as <c>partitionKey</c>.
/// </summary>
internal sealed class CosmosSystemTextJsonSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek && stream.Length == 0)
            {
                return default!;
            }

            return JsonSerializer.Deserialize<T>(stream, _options)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, _options);
        stream.Position = 0;
        return stream;
    }
}
