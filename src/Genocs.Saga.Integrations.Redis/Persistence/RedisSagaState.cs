using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaState : ISagaState
{
    public SagaId? Id { get; }
    public Type Type { get; }
    public SagaProcessState State { get; private set; }
    public object? Data { get; private set; }
    public Type? DataType { get; }

    [JsonConstructor]
    public RedisSagaState(SagaId id, Type type, SagaProcessState state, object? data = null, Type? dataType = null)
        => (Id, Type, State, Data, DataType) = (id, type, state, data, dataType);

    public static ISagaState Create(SagaId sagaId, Type sagaType, SagaProcessState state, object? data = null, Type? dataType = null)
        => new RedisSagaState(sagaId, sagaType, state, data, dataType);

    public void Update(SagaProcessState state, object? data = null)
    {
        State = state;
        Data = data;
    }
}