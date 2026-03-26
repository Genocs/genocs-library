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
        SagaPostProcessor postProcessor = new(log);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor);

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
        SagaPostProcessor postProcessor = new(log);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor);

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
        logEntries[0].Outcome.ShouldBe(SagaLogEntryOutcome.Completed);
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
        SagaPostProcessor postProcessor = new(log);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor);

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
        SagaPostProcessor postProcessor = new(log);
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor);

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
        SagaPostProcessor postProcessor = new(new InMemorySagaLog());
        SagaCoordinator coordinator = new(seeker, initializer, processor, postProcessor);

        ISagaContext context = SagaContext.Create()
            .WithSagaId("conflict-saga")
            .WithOriginator("tests")
            .Build();

        await Should.ThrowAsync<SagaConcurrencyException>(() => coordinator.ProcessAsync(new ConcurrencyConflictMessage(), context: context));
    }

    private sealed class FailingMessage;
    private sealed class StartMessage;
    private sealed class FailingFollowUpMessage;
    private sealed class SharedMessage;
    private sealed class CompletedStartMessage;
    private sealed class CompletedFollowUpMessage;
    private sealed class ConcurrencyConflictMessage;

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

    private sealed class AlwaysConflictingSagaStateRepository : ISagaStateRepository
    {
        public Task<ISagaState?> ReadAsync(SagaId id, Type type)
            => Task.FromResult<ISagaState?>(null);

        public Task WriteAsync(ISagaState state)
            => throw new SagaConcurrencyException("Simulated optimistic concurrency conflict.");
    }
}
