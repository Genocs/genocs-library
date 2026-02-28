using MongoDB.Bson.Serialization.Attributes;

namespace Genocs.Saga.Integrations.MongoDB.Persistence;

internal class MongoSagaState : ISagaState
{
    [BsonId]
    [BsonElement("Id")]
    public string? MongoId { get; set; }

    [BsonIgnore]
    public SagaId? Id => MongoId;

    public string? SagaType { get; set; }
    public SagaProcessState State { get; set; }
    public object? Data { get; set; }

    Type? ISagaState.Type => _type ??= AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(SagaType))
            ?.FirstOrDefault(t => t is {});

    private Type? _type;

    public void Update(SagaProcessState state, object? data = null)
    {
        State = state;
        Data = data;
    }
}
