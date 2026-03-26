using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaLogData : ISagaLogData
{
    public SagaId Id { get; }
    public Type Type { get; }
    public long CreatedAt { get; }
    public object Message { get; }
    public Type MessageType { get; }
    public SagaLogEntryOutcome Outcome { get; }

    [JsonConstructor]
    public RedisSagaLogData(SagaId id, Type type, long createdAt, object message, Type messageType, SagaLogEntryOutcome outcome)
    {
        Id = id;
        Type = type;
        CreatedAt = createdAt;
        Message = message;
        MessageType = messageType;
        Outcome = outcome;
    }

    public static ISagaLogData Create(SagaId sagaId, Type sagaType, object message, SagaLogEntryOutcome outcome)
        => new RedisSagaLogData(sagaId, sagaType, DateTimeOffset.Now.ToUnixTimeMilliseconds(), message, message.GetType(), outcome);
}