namespace Genocs.Saga.Async;

internal interface ISagaExecutionLock
{
    Task<IDisposable> LockAsync(SagaId sagaId);
}