namespace Genocs.Saga.Managers;

internal interface ISagaInitializer
{
    Task<(bool IsInitialized, ISagaState State)> TryInitializeAsync<TMessage>(ISaga saga, SagaId id, TMessage _);
}
