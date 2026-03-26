using Genocs.Saga.Persistence;

namespace Genocs.Saga.Managers;

internal sealed class SagaProcessor : ISagaProcessor
{
    private readonly ISagaStateRepository _repository;
    private readonly ISagaLog _log;

    public SagaProcessor(ISagaStateRepository repository, ISagaLog log)
    {
        _repository = repository;
        _log = log;
    }

    public async Task ProcessAsync<TMessage>(
        ISaga saga,
        TMessage message,
        ISagaState state,
        ISagaContext context)
        where TMessage : class
    {
        var action = (ISagaAction<TMessage>)saga;
        string sagaType = saga.GetType().Name;
        string messageType = typeof(TMessage).Name;
        SagaLogEntryOutcome outcome = SagaLogEntryOutcome.Completed;

        using var handleActivity = SagaTelemetry.StartHandleActivity(saga.Id, sagaType, messageType);

        try
        {
            await action.HandleAsync(message, context);
        }
        catch (Exception ex)
        {
            outcome = SagaLogEntryOutcome.Failed;
            context.SagaContextError = new SagaContextError(ex);

            if (saga.State is not SagaProcessState.Rejected)
            {
                try
                {
                    saga.Reject(ex);
                }
                catch (SagaException)
                {
                    // Reject transitions the saga to Rejected and throws by design.
                    // Swallowing here keeps the pipeline alive so post-processing can compensate.
                }
            }
        }
        finally
        {
            await UpdateSagaAsync(message, saga, state, outcome);
        }
    }

    private async Task UpdateSagaAsync<TMessage>(TMessage message, ISaga saga, ISagaState state, SagaLogEntryOutcome outcome)
        where TMessage : class
    {
        var sagaType = saga.GetType();

        object? updatedSagaData = sagaType.GetProperty(nameof(ISaga<object>.Data))?.GetValue(saga);

        state.Update(saga.State, updatedSagaData);
        var logData = SagaLogData.Create(saga.Id, sagaType, message, outcome);

        var persistenceTasks = new[]
        {
            _repository.WriteAsync(state),
            _log.WriteAsync(logData)
        };

        await Task.WhenAll(persistenceTasks).ConfigureAwait(false);
    }
}
