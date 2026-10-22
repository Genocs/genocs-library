using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Genocs.Saga.Integrations.MongoDB.Persistence;

internal class MongoSagaLogData : ISagaLogData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string MongoId { get; set; }

    public string EntryId { get; set; } = string.Empty;

    public required string SagaId { get; init; }

    [BsonIgnore]
    public SagaId Id => SagaId;

    public string? SagaType { get; init; }

    public long CreatedAt { get; init; }

    public object? Message { get; init; }

    public string? MessageId { get; init; }

    public SagaLogEntryOutcome Outcome { get; set; }

    Type? ISagaLogData.Type
        => string.IsNullOrWhiteSpace(SagaType) ? null : Assembly.GetEntryAssembly()?.GetType(SagaType);
}
