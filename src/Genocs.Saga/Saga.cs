namespace Genocs.Saga;

public abstract class Saga : ISaga
{
    public SagaId Id { get; private set; }

    public SagaProcessState State { get; protected set; }

    public virtual void Initialize(SagaId id, SagaProcessState state)
        => (Id, State) = (id, state);

    public virtual SagaId ResolveId(object message, ISagaContext context)
        => context.SagaId;

    public virtual void Complete()
        => State = SagaProcessState.Completed;

    public virtual Task CompleteAsync()
    {
        Complete();
        return Task.CompletedTask;
    }

    public virtual void Reject(Exception? innerException = null)
    {
        State = SagaProcessState.Rejected;
        throw new SagaException("Saga rejection called by method", innerException);
    }

    public virtual Task RejectAsync(Exception? innerException = null)
    {
        Reject(innerException);
        return Task.CompletedTask;
    }
}

public abstract class Saga<TData> : Saga, ISaga<TData>
    where TData : class, new()
{
    public TData Data { get; set; }

    public virtual void Initialize(SagaId id, SagaProcessState state, TData data)
    {
        base.Initialize(id, state);
        Data = data;
    }
}
