using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaLogData : ISagaLogData
{
    public string EntryId { get; }
    public SagaId Id { get; }
    public Type Type { get; }
    public long CreatedAt { get; }
    public object Message { get; }
    public Type MessageType { get; }
    public string MessageId { get; }
    public SagaLogEntryOutcome Outcome { get; }

    [JsonConstructor]
    public RedisSagaLogData(string entryId, SagaId id, Type type, long createdAt, object message, Type messageType, string messageId, SagaLogEntryOutcome outcome)
    {
        EntryId = entryId;
        Id = id;
        Type = type;
        CreatedAt = createdAt;
        Message = message;
        MessageType = messageType;
        MessageId = messageId;
        Outcome = outcome;
    }

    public static RedisSagaLogData Create(SagaId sagaId, Type sagaType, object message, SagaLogEntryOutcome outcome, string? messageId = null, string? entryId = null)
        => new(entryId ?? Guid.NewGuid().ToString("N"), sagaId, sagaType, DateTimeOffset.Now.ToUnixTimeMilliseconds(), message, message.GetType(), messageId, outcome);
}