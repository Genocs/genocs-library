using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaState : ISagaState
{
    public SagaId? Id { get; }
    public Type Type { get; }
    public SagaProcessState State { get; private set; }
    public object? Data { get; private set; }
    public long Version { get; private set; }
    public Type? DataType { get; }

    [JsonConstructor]
    public RedisSagaState(SagaId id, Type type, SagaProcessState state, object? data = null, long version = 0, Type? dataType = null)
        => (Id, Type, State, Data, Version, DataType) = (id, type, state, data, version, dataType);

    public static ISagaState Create(SagaId sagaId, Type sagaType, SagaProcessState state, object? data = null, long version = 0, Type? dataType = null)
        => new RedisSagaState(sagaId, sagaType, state, data, version, dataType);

    public void Update(SagaProcessState state, object? data = null)
    {
        State = state;
        Data = data;
    }

    public void UpdateVersion(long version)
        => Version = version;
}