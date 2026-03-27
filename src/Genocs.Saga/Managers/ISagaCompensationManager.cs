namespace Genocs.Saga.Managers;

internal interface ISagaCompensationManager
{
    Task CompensateAsync(ISaga saga, ISagaContext context);

    Task RetryAsync<TSaga>(SagaId sagaId, ISagaContext? context = null)
        where TSaga : class, ISaga;
}