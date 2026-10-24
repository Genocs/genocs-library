using Genocs.Saga.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Genocs.Saga.Managers;

internal sealed class SagaCompensationManager : ISagaCompensationManager
{
    private const string RecoveryOriginator = "Genocs.Saga.Recovery";

    private readonly ISagaLog _log;
    private readonly ISagaStateRepository _repository;
    private readonly IServiceProvider _serviceProvider;

    public SagaCompensationManager(ISagaLog log, ISagaStateRepository repository, IServiceProvider serviceProvider)
        => (_log, _repository, _serviceProvider) = (log, repository, serviceProvider);

    public Task CompensateAsync(ISaga saga, ISagaContext context)
        => CompensateCoreAsync(saga, saga.GetType(), context, includeFailedCompensations: false);

    public async Task RetryAsync<TSaga>(SagaId sagaId, ISagaContext? context = null)
        where TSaga : class, ISaga
    {
        Type sagaType = typeof(TSaga);
        ISagaState persistedState = await _repository.ReadAsync(sagaId, sagaType).ConfigureAwait(false)
            ?? throw new SagaException($"Saga state for '{sagaType.FullName}' and id '{sagaId.Id}' was not found.");

        if (persistedState.State is not SagaProcessState.CompensationFailed)
        {
            throw new SagaException($"Saga '{sagaType.FullName}' with id '{sagaId.Id}' is in state '{persistedState.State}' and cannot retry compensation. Expected '{SagaProcessState.CompensationFailed}'.");
        }

        ISaga saga = (ISaga)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, sagaType);
        InitializeSaga(saga, sagaId, persistedState);

        ISagaContext recoveryContext = EnsureRecoveryContext(sagaId, context);
        await CompensateCoreAsync(saga, sagaType, recoveryContext, includeFailedCompensations: true).ConfigureAwait(false);
    }

    private async Task CompensateCoreAsync(ISaga saga, Type sagaType, ISagaContext context, bool includeFailedCompensations)
    {
        using var compensateActivity = SagaTelemetry.StartCompensateActivity(saga.Id, sagaType.Name);
        await UpdateSagaStateAsync(saga, sagaType, SagaProcessState.Compensating).ConfigureAwait(false);

        SagaLogEntryOutcome[] eligibleOutcomes = includeFailedCompensations
            ? [SagaLogEntryOutcome.Completed, SagaLogEntryOutcome.CompensationFailed]
            : [SagaLogEntryOutcome.Completed];

        var sagaLogs = await _log.ReadAsync(saga.Id, sagaType).ConfigureAwait(false);

        foreach (ISagaLogData logEntry in sagaLogs
                     .Where(l => eligibleOutcomes.Contains(l.Outcome))
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
        InitializeSaga(saga, saga.Id, persistedState);
    }

    private static void InitializeSaga(ISaga saga, SagaId sagaId, ISagaState state)
    {
        if (state.Data is null)
        {
            saga.Initialize(sagaId, state.State);
            return;
        }

        saga.InvokeGeneric(nameof(ISaga<>.Initialize), sagaId, state.State, state.Data);
    }

    private static ISagaContext EnsureRecoveryContext(SagaId sagaId, ISagaContext? context)
    {
        if (context is null)
        {
            return SagaContext.Create()
                .WithSagaId(sagaId)
                .WithOriginator(RecoveryOriginator)
                .Build();
        }

        if (context.SagaId != sagaId)
        {
            throw new SagaException($"Recovery context saga id '{context.SagaId.Id}' does not match target saga id '{sagaId.Id}'.");
        }

        return context;
    }
}