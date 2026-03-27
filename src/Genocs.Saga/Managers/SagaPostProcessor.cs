using Genocs.Saga.Utils;
using System.Reflection;

namespace Genocs.Saga.Managers;

internal sealed class SagaPostProcessor : ISagaPostProcessor
{
    private readonly ISagaLog _log;
    private readonly ISagaStateRepository _repository;

    public SagaPostProcessor(ISagaLog log, ISagaStateRepository repository)
        => (_log, _repository) = (log, repository);

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

    private async Task CompensateAsync(ISaga saga, Type sagaType, ISagaContext context)
    {
        using var compensateActivity = SagaTelemetry.StartCompensateActivity(saga.Id, sagaType.Name);
        await UpdateSagaStateAsync(saga, sagaType, SagaProcessState.Compensating).ConfigureAwait(false);

        var sagaLogs = await _log.ReadAsync(saga.Id, sagaType);

        foreach (ISagaLogData logEntry in sagaLogs
                     .Where(l => l.Outcome is SagaLogEntryOutcome.Completed)
                     .OrderByDescending(l => l.CreatedAt))
        {
            try
            {
                object compensationResult = saga.InvokeGeneric(nameof(ISagaAction<object>.CompensateAsync), logEntry.Message, context)
                    ?? throw new SagaException($"Compensation method was not found for '{sagaType.FullName}'.");

                if (compensationResult is not Task compensationTask)
                {
                    throw new SagaException($"Compensation method for '{sagaType.FullName}' did not return a task.");
                }

                await compensationTask.ConfigureAwait(false);
                await _log.UpdateOutcomeAsync(saga.Id, sagaType, logEntry.EntryId, SagaLogEntryOutcome.Compensated)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Exception failure = ex is TargetInvocationException { InnerException: Exception innerException }
                    ? innerException
                    : ex;

                context.SagaContextError = new SagaContextError(failure);
                await _log.UpdateOutcomeAsync(saga.Id, sagaType, logEntry.EntryId, SagaLogEntryOutcome.CompensationFailed)
                    .ConfigureAwait(false);
                await UpdateSagaStateAsync(saga, sagaType, SagaProcessState.CompensationFailed).ConfigureAwait(false);
                throw new SagaException("Saga compensation failed.", failure);
            }
        }

        await UpdateSagaStateAsync(saga, sagaType, SagaProcessState.Compensated).ConfigureAwait(false);
    }

    private async Task UpdateSagaStateAsync(ISaga saga, Type sagaType, SagaProcessState state)
    {
        ISagaState persistedState = await _repository.ReadAsync(saga.Id, sagaType).ConfigureAwait(false)
            ?? throw new SagaException($"Saga state for '{sagaType.FullName}' and id '{saga.Id.Id}' was not found.");

        persistedState.Update(state, persistedState.Data);
        await _repository.WriteAsync(persistedState).ConfigureAwait(false);
        InitializeSaga(saga, persistedState);
    }

    private static void InitializeSaga(ISaga saga, ISagaState state)
    {
        if (state.Data is null)
        {
            saga.Initialize(saga.Id, state.State);
            return;
        }

        saga.InvokeGeneric(nameof(ISaga<>.Initialize), saga.Id, state.State, state.Data);
    }
}
