namespace Genocs.Saga.Persistence;

internal class SagaState : ISagaState
{
    public SagaId? Id { get; }
    public Type? Type { get; }
    public SagaProcessState State { get; private set; }
    public object? Data { get; private set; }

    private SagaState(SagaId? id, Type type, SagaProcessState state, object? data)
        => (Id, Type, State, Data) = (id, type, state, data);

    public static ISagaState Create(SagaId? id, Type type, SagaProcessState state, object? data = null)
        => new SagaState(id, type, state, data);

    public void Update(SagaProcessState state, object? data = null)
    {
        State = state;
        Data = data;
    }
}
