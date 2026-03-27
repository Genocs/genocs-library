using Genocs.Saga.Managers;
using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Managers;

public class SagaCoordinatorTests
{
    [Fact]
    public async Task ProcessAsync_WhenHandleThrows_ShouldRejectWithoutCompensatingTheFailedStep()
    {
        ServiceCollection services = new();
        services.AddSingleton<ThrowingSaga>();
        services.AddSingleton<ISagaStartAction<FailingMessage>>(sp => sp.GetRequiredService<ThrowingSaga>());
        services.AddSingleton<ISagaAction<FailingMessage>>(sp => sp.GetRequiredService<ThrowingSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Empty;
        int rejectedHookCalls = 0;
        ISagaContext? rejectedContext = null;

        await coordinator.ProcessAsync(
            new FailingMessage(),
            onRejected: (_, ctx) =>
            {
                rejectedHookCalls++;
                rejectedContext = ctx;
                return Task.CompletedTask;
            },
            context: context);

        rejectedHookCalls.ShouldBe(1);
        rejectedContext.ShouldNotBeNull();
        rejectedContext.SagaContextError.ShouldNotBeNull();
        rejectedContext.SagaContextError.Exception.ShouldBeOfType<InvalidOperationException>();
        context.SagaContextError.ShouldBeNull();

        ThrowingSaga saga = serviceProvider.GetRequiredService<ThrowingSaga>();
        saga.CompensatedMessages.ShouldBe(0);

        var logEntries = (await log.ReadAsync(context.SagaId, typeof(ThrowingSaga))).ToList();
        logEntries.Count.ShouldBe(1);
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.Failed);
    }

    [Fact]
    public async Task ProcessAsync_WhenLaterStepFails_ShouldCompensateOnlyPreviouslyCompletedSteps()
    {
        ServiceCollection services = new();
        services.AddSingleton<TwoStepSaga>();
        services.AddSingleton<ISagaStartAction<StartMessage>>(sp => sp.GetRequiredService<TwoStepSaga>());
        services.AddSingleton<ISagaAction<FailingFollowUpMessage>>(sp => sp.GetRequiredService<TwoStepSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext startContext = SagaContext.Create()
            .WithSagaId("saga-1")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new StartMessage(), context: startContext);

        ISagaContext failingContext = SagaContext.Create()
            .WithSagaId("saga-1")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new FailingFollowUpMessage(), context: failingContext);

        TwoStepSaga saga = serviceProvider.GetRequiredService<TwoStepSaga>();
        saga.StartCompensationCalls.ShouldBe(1);
        saga.FailingCompensationCalls.ShouldBe(0);

        var logEntries = (await log.ReadAsync("saga-1", typeof(TwoStepSaga))).OrderBy(l => l.CreatedAt).ToList();
        logEntries.Count.ShouldBe(2);
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.Compensated);
        logEntries[1].Outcome.ShouldBe(SagaLogEntryOutcome.Failed);
    }

    [Fact]
    public async Task ProcessAsync_WhenMultipleSagasHandleSameMessage_ShouldUseIsolatedExecutionContexts()
    {
        ServiceCollection services = new();
        services.AddSingleton<SuccessfulSharedSaga>();
        services.AddSingleton<FailingSharedSaga>();
        services.AddSingleton<ISagaStartAction<SharedMessage>>(sp => sp.GetRequiredService<SuccessfulSharedSaga>());
        services.AddSingleton<ISagaStartAction<SharedMessage>>(sp => sp.GetRequiredService<FailingSharedSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext inputContext = SagaContext.Create()
            .WithSagaId("shared-saga")
            .WithOriginator("tests")
            .Build();

        ISagaContext? successCallbackContext = null;
        ISagaContext? rejectionCallbackContext = null;

        await coordinator.ProcessAsync(
            new SharedMessage(),
            onCompleted: (_, ctx) =>
            {
                successCallbackContext = ctx;
                return Task.CompletedTask;
            },
            onRejected: (_, ctx) =>
            {
                rejectionCallbackContext = ctx;
                return Task.CompletedTask;
            },
            context: inputContext);

        successCallbackContext.ShouldNotBeNull();
        successCallbackContext.SagaContextError.ShouldBeNull();

        rejectionCallbackContext.ShouldNotBeNull();
        rejectionCallbackContext.SagaContextError.ShouldNotBeNull();
        rejectionCallbackContext.SagaContextError.Exception.ShouldBeOfType<InvalidOperationException>();

        inputContext.SagaContextError.ShouldBeNull();
        ReferenceEquals(successCallbackContext, rejectionCallbackContext).ShouldBeFalse();
        ReferenceEquals(inputContext, successCallbackContext).ShouldBeFalse();
        ReferenceEquals(inputContext, rejectionCallbackContext).ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessAsync_WhenSagaIsCompleted_ShouldIgnoreFollowUpMessages()
    {
        ServiceCollection services = new();
        services.AddSingleton<CompletedSaga>();
        services.AddSingleton<ISagaStartAction<CompletedStartMessage>>(sp => sp.GetRequiredService<CompletedSaga>());
        services.AddSingleton<ISagaAction<CompletedFollowUpMessage>>(sp => sp.GetRequiredService<CompletedSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("completed-saga")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new CompletedStartMessage(), context: context);
        await coordinator.ProcessAsync(new CompletedFollowUpMessage(), context: context);

        CompletedSaga saga = serviceProvider.GetRequiredService<CompletedSaga>();
        saga.FollowUpHandleCalls.ShouldBe(0);

        var logEntries = (await log.ReadAsync("completed-saga", typeof(CompletedSaga))).ToList();
        logEntries.Count.ShouldBe(1);
        logEntries[0].Message.ShouldBeOfType<CompletedStartMessage>();
    }

    [Fact]
    public async Task ProcessAsync_WhenStateWriteConflicts_ShouldPropagateSagaConcurrencyException()
    {
        ServiceCollection services = new();
        services.AddSingleton<ConcurrencyConflictSaga>();
        services.AddSingleton<ISagaStartAction<ConcurrencyConflictMessage>>(sp => sp.GetRequiredService<ConcurrencyConflictSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(new AlwaysConflictingSagaStateRepository());
        SagaProcessor processor = new(new AlwaysConflictingSagaStateRepository(), new InMemorySagaLog());
        SagaCompensationManager compensationManager = new(new InMemorySagaLog(), new AlwaysConflictingSagaStateRepository(), serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("conflict-saga")
            .WithOriginator("tests")
            .Build();

        await Should.ThrowAsync<SagaConcurrencyException>(() => coordinator.ProcessAsync(new ConcurrencyConflictMessage(), context: context));
    }

    [Fact]
    public async Task ProcessAsync_WhenDuplicateMessageIdIsProcessed_ShouldSkipSecondDelivery()
    {
        ServiceCollection services = new();
        services.AddSingleton<IdempotentSaga>();
        services.AddSingleton<ISagaStartAction<IdempotentMessage>>(sp => sp.GetRequiredService<IdempotentSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("duplicate-saga")
            .WithOriginator("tests")
            .Build();

        IdempotentMessage message = new("msg-1");

        await coordinator.ProcessAsync(message, context: context);
        await coordinator.ProcessAsync(message, context: context);

        IdempotentSaga saga = serviceProvider.GetRequiredService<IdempotentSaga>();
        saga.HandleCalls.ShouldBe(1);

        var logEntries = (await log.ReadAsync("duplicate-saga", typeof(IdempotentSaga))).ToList();
        logEntries.Count.ShouldBe(1);
        logEntries[0].MessageId.ShouldBe("msg-1");
    }

    [Fact]
    public async Task ProcessAsync_WhenCompensationSucceeds_ShouldPersistCompensatedState()
    {
        ServiceCollection services = new();
        services.AddSingleton<CompensatingSaga>();
        services.AddSingleton<ISagaStartAction<CompensatingStartMessage>>(sp => sp.GetRequiredService<CompensatingSaga>());
        services.AddSingleton<ISagaAction<CompensatingFailMessage>>(sp => sp.GetRequiredService<CompensatingSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("compensated-saga")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new CompensatingStartMessage("start-1"), context: context);
        await coordinator.ProcessAsync(new CompensatingFailMessage("fail-1"), context: context);

        ISagaState? persisted = await repository.ReadAsync("compensated-saga", typeof(CompensatingSaga));
        persisted.ShouldNotBeNull();
        persisted.State.ShouldBe(SagaProcessState.Compensated);

        var logEntries = (await log.ReadAsync("compensated-saga", typeof(CompensatingSaga))).OrderBy(l => l.CreatedAt).ToList();
        logEntries.Count.ShouldBe(2);
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.Compensated);
        logEntries[1].Outcome.ShouldBe(SagaLogEntryOutcome.Failed);
    }

    [Fact]
    public async Task ProcessAsync_WhenCompensationFails_ShouldPersistCompensationFailedState()
    {
        ServiceCollection services = new();
        services.AddSingleton<CompensationFailureSaga>();
        services.AddSingleton<ISagaStartAction<CompensationFailureStartMessage>>(sp => sp.GetRequiredService<CompensationFailureSaga>());
        services.AddSingleton<ISagaAction<CompensationFailureTriggerMessage>>(sp => sp.GetRequiredService<CompensationFailureSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("compensation-failure-saga")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new CompensationFailureStartMessage("start-1"), context: context);

        SagaException exception = await Should.ThrowAsync<SagaException>(() => coordinator.ProcessAsync(new CompensationFailureTriggerMessage("fail-1"), context: context));

        exception.InnerException.ShouldBeOfType<InvalidOperationException>();

        ISagaState? persisted = await repository.ReadAsync("compensation-failure-saga", typeof(CompensationFailureSaga));
        persisted.ShouldNotBeNull();
        persisted.State.ShouldBe(SagaProcessState.CompensationFailed);

        var logEntries = (await log.ReadAsync("compensation-failure-saga", typeof(CompensationFailureSaga))).OrderBy(l => l.CreatedAt).ToList();
        logEntries.Count.ShouldBe(2);
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.CompensationFailed);
        logEntries[1].Outcome.ShouldBe(SagaLogEntryOutcome.Failed);
    }

    [Fact]
    public async Task RetryCompensationAsync_WhenCompensationPreviouslyFailed_ShouldRetryAndPersistCompensatedState()
    {
        ServiceCollection services = new();
        services.AddSingleton<RetriableCompensationSaga>();
        services.AddSingleton<ISagaStartAction<RetriableStartMessage>>(sp => sp.GetRequiredService<RetriableCompensationSaga>());
        services.AddSingleton<ISagaAction<RetriableFailMessage>>(sp => sp.GetRequiredService<RetriableCompensationSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("retry-compensation-saga")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new RetriableStartMessage("start-1"), context: context);
        await Should.ThrowAsync<SagaException>(() => coordinator.ProcessAsync(new RetriableFailMessage("fail-1"), context: context));

        await coordinator.RetryCompensationAsync<RetriableCompensationSaga>("retry-compensation-saga");

        RetriableCompensationSaga saga = serviceProvider.GetRequiredService<RetriableCompensationSaga>();
        saga.StartCompensationCalls.ShouldBe(2);

        ISagaState? persisted = await repository.ReadAsync("retry-compensation-saga", typeof(RetriableCompensationSaga));
        persisted.ShouldNotBeNull();
        persisted.State.ShouldBe(SagaProcessState.Compensated);

        var logEntries = (await log.ReadAsync("retry-compensation-saga", typeof(RetriableCompensationSaga))).OrderBy(l => l.CreatedAt).ToList();
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.Compensated);
        logEntries[1].Outcome.ShouldBe(SagaLogEntryOutcome.Failed);
    }

    [Fact]
    public async Task RetryCompensationAsync_WhenSagaIsNotInCompensationFailedState_ShouldThrowSagaException()
    {
        ServiceCollection services = new();
        services.AddSingleton<CompensatingSaga>();
        services.AddSingleton<ISagaStartAction<CompensatingStartMessage>>(sp => sp.GetRequiredService<CompensatingSaga>());

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        InMemorySagaStateRepository repository = new();
        InMemorySagaLog log = new();
        SagaSeeker seeker = new(serviceProvider);
        SagaInitializer initializer = new(repository);
        SagaProcessor processor = new(repository, log);
        SagaCompensationManager compensationManager = new(log, repository, serviceProvider);
        SagaPostProcessor postProcessor = new(compensationManager);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor, compensationManager);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("invalid-retry-saga")
            .WithOriginator("tests")
            .Build();

        await coordinator.ProcessAsync(new CompensatingStartMessage("start-1"), context: context);

        SagaException exception = await Should.ThrowAsync<SagaException>(() => coordinator.RetryCompensationAsync<CompensatingSaga>("invalid-retry-saga"));
        exception.Message.ShouldContain(SagaProcessState.CompensationFailed.ToString());
    }

    private sealed class FailingMessage;
    private sealed class StartMessage;
    private sealed class FailingFollowUpMessage;
    private sealed class SharedMessage;
    private sealed class CompletedStartMessage;
    private sealed class CompletedFollowUpMessage;
    private sealed class ConcurrencyConflictMessage;
    private sealed record IdempotentMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record CompensatingStartMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record CompensatingFailMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record CompensationFailureStartMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record CompensationFailureTriggerMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record RetriableStartMessage(string MessageId) : ISagaMessageIdentity;
    private sealed record RetriableFailMessage(string MessageId) : ISagaMessageIdentity;

    private sealed class ThrowingSaga : Saga, ISagaStartAction<FailingMessage>
    {
        public int CompensatedMessages { get; private set; }

        public Task HandleAsync(FailingMessage message, ISagaContext context)
            => throw new InvalidOperationException("Simulated exception in HandleAsync");

        public Task CompensateAsync(FailingMessage message, ISagaContext context)
        {
            CompensatedMessages++;
            return Task.CompletedTask;
        }
    }

    private sealed class TwoStepSaga : Saga,
        ISagaStartAction<StartMessage>,
        ISagaAction<FailingFollowUpMessage>
    {
        public int StartCompensationCalls { get; private set; }
        public int FailingCompensationCalls { get; private set; }

        public Task HandleAsync(StartMessage message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task CompensateAsync(StartMessage message, ISagaContext context)
        {
            StartCompensationCalls++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(FailingFollowUpMessage message, ISagaContext context)
            => throw new InvalidOperationException("Simulated failure after a successful prior step");

        public Task CompensateAsync(FailingFollowUpMessage message, ISagaContext context)
        {
            FailingCompensationCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class SuccessfulSharedSaga : Saga, ISagaStartAction<SharedMessage>
    {
        public Task HandleAsync(SharedMessage message, ISagaContext context)
        {
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(SharedMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class FailingSharedSaga : Saga, ISagaStartAction<SharedMessage>
    {
        public Task HandleAsync(SharedMessage message, ISagaContext context)
            => throw new InvalidOperationException("Shared saga failure");

        public Task CompensateAsync(SharedMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class CompletedSaga : Saga,
        ISagaStartAction<CompletedStartMessage>,
        ISagaAction<CompletedFollowUpMessage>
    {
        public int FollowUpHandleCalls { get; private set; }

        public Task HandleAsync(CompletedStartMessage message, ISagaContext context)
        {
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CompletedStartMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task HandleAsync(CompletedFollowUpMessage message, ISagaContext context)
        {
            FollowUpHandleCalls++;
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CompletedFollowUpMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class ConcurrencyConflictSaga : Saga, ISagaStartAction<ConcurrencyConflictMessage>
    {
        public Task HandleAsync(ConcurrencyConflictMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(ConcurrencyConflictMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class IdempotentSaga : Saga, ISagaStartAction<IdempotentMessage>
    {
        public int HandleCalls { get; private set; }

        public Task HandleAsync(IdempotentMessage message, ISagaContext context)
        {
            HandleCalls++;
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IdempotentMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class CompensatingSaga : Saga,
        ISagaStartAction<CompensatingStartMessage>,
        ISagaAction<CompensatingFailMessage>
    {
        public Task HandleAsync(CompensatingStartMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(CompensatingStartMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task HandleAsync(CompensatingFailMessage message, ISagaContext context)
            => throw new InvalidOperationException("Compensation should run.");

        public Task CompensateAsync(CompensatingFailMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class CompensationFailureSaga : Saga,
        ISagaStartAction<CompensationFailureStartMessage>,
        ISagaAction<CompensationFailureTriggerMessage>
    {
        public Task HandleAsync(CompensationFailureStartMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(CompensationFailureStartMessage message, ISagaContext context)
            => throw new InvalidOperationException("Compensation failure.");

        public Task HandleAsync(CompensationFailureTriggerMessage message, ISagaContext context)
            => throw new InvalidOperationException("Trigger rejection.");

        public Task CompensateAsync(CompensationFailureTriggerMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class RetriableCompensationSaga : Saga,
        ISagaStartAction<RetriableStartMessage>,
        ISagaAction<RetriableFailMessage>
    {
        public int StartCompensationCalls { get; private set; }

        public Task HandleAsync(RetriableStartMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(RetriableStartMessage message, ISagaContext context)
        {
            StartCompensationCalls++;

            if (StartCompensationCalls == 1)
            {
                throw new InvalidOperationException("First compensation attempt failed.");
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(RetriableFailMessage message, ISagaContext context)
            => throw new InvalidOperationException("Trigger retryable compensation failure.");

        public Task CompensateAsync(RetriableFailMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class AlwaysConflictingSagaStateRepository : ISagaStateRepository
    {
        public Task<ISagaState?> ReadAsync(SagaId id, Type type)
            => Task.FromResult<ISagaState?>(null);

        public Task WriteAsync(ISagaState state)
            => throw new SagaConcurrencyException("Simulated optimistic concurrency conflict.");
    }
}
