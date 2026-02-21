using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaLogData : ISagaLogData
{
    public SagaId Id { get; }
    public Type Type { get; }
    public long CreatedAt { get; }
    public object Message { get; }
    public Type MessageType { get; }

    [JsonConstructor]
    public RedisSagaLogData(SagaId id, Type type, long createdAt, object message, Type messageType)
    {
        Id = id;
        Type = type;
        CreatedAt = createdAt;
        Message = message;
        MessageType = messageType;
    }

    public static ISagaLogData Create(SagaId sagaId, Type sagaType, object message)
        => new RedisSagaLogData(sagaId, sagaType, DateTimeOffset.Now.ToUnixTimeMilliseconds(), message, message.GetType());
}