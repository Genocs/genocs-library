namespace Genocs.Saga.Async;

internal sealed class InProcessSagaExecutionLock : ISagaExecutionLock
{
    private readonly KeyedLocker _locker = new();

    public Task<IDisposable> LockAsync(SagaId sagaId)
        => _locker.LockAsync(sagaId);
}