namespace Genocs.Saga.Managers;

internal sealed class SagaPostProcessor : ISagaPostProcessor
{
    private readonly ISagaCompensationManager _compensationManager;

    public SagaPostProcessor(ISagaCompensationManager compensationManager)
        => _compensationManager = compensationManager;

    public async Task ProcessAsync<TMessage>(
        ISaga saga,
        TMessage message,
        ISagaContext context,
        Func<TMessage, ISagaContext, Task> onCompleted,
        Func<TMessage, ISagaContext, Task> onRejected)
    {
        var sagaType = saga.GetType();

        switch (saga.State)
        {
            case SagaProcessState.Rejected:
                await onRejected(message, context);
                await CompensateAsync(saga, sagaType, context);
                break;
            case SagaProcessState.Completed:
                await onCompleted(message, context);
                break;
        }
    }

    private Task CompensateAsync(ISaga saga, Type sagaType, ISagaContext context)
        => _compensationManager.CompensateAsync(saga, context);
}
