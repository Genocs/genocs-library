using Genocs.Saga.Managers;
using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Managers;

public class SagaCoordinatorTests
{
    [Fact]
    public async Task ProcessAsync_WhenHandleThrows_ShouldRejectAndCompensate()
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

        await coordinator.ProcessAsync(
            new FailingMessage(),
            onRejected: (_, _) =>
            {
                rejectedHookCalls++;
                return Task.CompletedTask;
            },
            context: context);

        rejectedHookCalls.ShouldBe(1);
        context.SagaContextError.ShouldNotBeNull();
        context.SagaContextError.Exception.ShouldBeOfType<InvalidOperationException>();

        ThrowingSaga saga = serviceProvider.GetRequiredService<ThrowingSaga>();
        saga.CompensatedMessages.ShouldBe(1);
    }

    private sealed class FailingMessage;

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
}
