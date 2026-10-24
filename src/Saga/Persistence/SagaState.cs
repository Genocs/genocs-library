namespace Genocs.Saga.Persistence;

internal class SagaState : ISagaState
{
    public SagaId? Id { get; }
    public Type? Type { get; }
    public SagaProcessState State { get; private set; }
    public object? Data { get; private set; }
    public long Version { get; private set; }

    private SagaState(SagaId? id, Type type, SagaProcessState state, object? data, long version)
        => (Id, Type, State, Data, Version) = (id, type, state, data, version);

    public static SagaState Create(SagaId? id, Type type, SagaProcessState state, object? data = null, long version = 0)
        => new SagaState(id, type, state, data, version);

    public static SagaState CopyOf(ISagaState state)
        => new SagaState(state.Id, state.Type!, state.State, state.Data, state.Version);

    public void Update(SagaProcessState state, object? data = null)
    {
        State = state;
        Data = data;
    }

    public void UpdateVersion(long version)
        => Version = version;
}
