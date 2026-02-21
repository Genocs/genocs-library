using Genocs.Saga.Utils;

namespace Genocs.Saga.Managers;

internal sealed class SagaPostProcessor : ISagaPostProcessor
{
    private readonly ISagaLog _log;

    public SagaPostProcessor(ISagaLog log)
    {
        _log = log;
    }

    public async Task ProcessAsync<TMessage>(ISaga saga, TMessage message, ISagaContext context,
        Func<TMessage, ISagaContext, Task> onCompleted, Func<TMessage, ISagaContext, Task> onRejected)
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

    private async Task CompensateAsync(ISaga saga, Type sagaType, ISagaContext context)
    {
        var sagaLogs = await _log.ReadAsync(saga.Id, sagaType);
        sagaLogs.OrderByDescending(l => l.CreatedAt)
            .Select(l => l.Message)
            .ToList()
            .ForEach(async message =>
            {
                await ((Task)saga.InvokeGeneric(nameof(ISagaAction<object>.CompensateAsync), message, context))
                    .ConfigureAwait(false);
            });
    }
}
