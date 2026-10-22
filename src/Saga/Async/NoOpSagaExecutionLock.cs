namespace Genocs.Saga.Async;

internal sealed class NoOpSagaExecutionLock : ISagaExecutionLock
{
    public Task<IDisposable> LockAsync(SagaId sagaId)
        => Task.FromResult<IDisposable>(NoOpDisposable.Instance);

    private sealed class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}