using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Core.Builders;
using Genocs.Messaging.CQRS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Messaging.UnitTests.CQRS;

public class ServiceBusMessageDispatcherTests
{
    [Fact]
    public async Task SendAsync_ForCommand_PublishesUsingCorrelationContext()
    {
        ServiceProvider serviceProvider = BuildProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var publisher = (FakeBusPublisher)serviceProvider.GetRequiredService<IBusPublisher>();
        var correlationAccessor = (FakeCorrelationContextAccessor)serviceProvider.GetRequiredService<ICorrelationContextAccessor>();

        var command = new TestCommand(Guid.NewGuid());
        await dispatcher.SendAsync(command);

        Assert.Equal(1, publisher.PublishCallCount);
        Assert.Same(command, publisher.LastMessage);
        Assert.Same(correlationAccessor.CorrelationContext, publisher.LastMessageContext);
    }

    [Fact]
    public async Task SendAsync_ForCommand_ForwardsCancellationToken()
    {
        ServiceProvider serviceProvider = BuildProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var publisher = (FakeBusPublisher)serviceProvider.GetRequiredService<IBusPublisher>();

        using var cts = new CancellationTokenSource();
        await dispatcher.SendAsync(new TestCommand(Guid.NewGuid()), cts.Token);

        Assert.Equal(cts.Token, publisher.LastCancellationToken);
    }

    [Fact]
    public async Task PublishAsync_ForEvent_ForwardsCancellationToken()
    {
        ServiceProvider serviceProvider = BuildProvider();
        var dispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
        var publisher = (FakeBusPublisher)serviceProvider.GetRequiredService<IBusPublisher>();

        using var cts = new CancellationTokenSource();
        await dispatcher.PublishAsync(new TestEvent("created"), cts.Token);

        Assert.Equal(cts.Token, publisher.LastCancellationToken);
    }

    [Fact]
    public async Task SendAsync_ForResultCommand_ThrowsClearNotSupportedException()
    {
        ServiceProvider serviceProvider = BuildProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var publisher = (FakeBusPublisher)serviceProvider.GetRequiredService<IBusPublisher>();

        var command = new TestResultCommand("check-stock");

        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => dispatcher.SendAsync<TestResultCommand, int>(command));

        Assert.Contains("Result-returning commands are not supported", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, publisher.PublishCallCount);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddGenocs(new ConfigurationBuilder().Build())
            .AddServiceBusCommandDispatcher()
            .AddServiceBusEventDispatcher();

        services.AddSingleton<IBusPublisher, FakeBusPublisher>();
        services.AddSingleton<ICorrelationContextAccessor>(new FakeCorrelationContextAccessor
        {
            CorrelationContext = new { CorrelationId = Guid.NewGuid().ToString("N") }
        });

        return services.BuildServiceProvider();
    }

    public sealed record TestCommand(Guid Id) : ICommand;

    public sealed record TestResultCommand(string Name) : ICommand<int>;

    public sealed record TestEvent(string Name) : IEvent;

    private sealed class FakeBusPublisher : IBusPublisher
    {
        public int PublishCallCount { get; private set; }

        public object? LastMessage { get; private set; }

        public object? LastMessageContext { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task PublishAsync<T>(
            T message,
            string? messageId = null,
            string? correlationId = null,
            string? spanContext = null,
            object? messageContext = null,
            IDictionary<string, object>? headers = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            PublishCallCount++;
            LastMessage = message;
            LastMessageContext = messageContext;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public object? CorrelationContext { get; set; }
    }
}
