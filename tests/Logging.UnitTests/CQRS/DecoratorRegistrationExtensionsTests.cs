using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Core.Builders;
using Genocs.Logging.CQRS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Logging.UnitTests.CQRS;

public class DecoratorRegistrationExtensionsTests
{
    [Fact]
    public async Task AddCommandHandlersLogging_WithExplicitAssembly_DecoratesAndExecutesHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<InvocationProbe>();
        services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        builder.AddCommandHandlersLogging(typeof(DecoratorRegistrationExtensionsTests).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        ICommandHandler<TestCommand> handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();
        var probe = provider.GetRequiredService<InvocationProbe>();

        Assert.Contains("CommandHandlerLoggingDecorator", handler.GetType().Name, StringComparison.Ordinal);

        await handler.HandleAsync(new TestCommand());

        Assert.Equal(1, probe.CommandInvocations);
    }

    [Fact]
    public async Task AddEventHandlersLogging_WithExplicitAssembly_DecoratesAndExecutesHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<InvocationProbe>();
        services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        builder.AddEventHandlersLogging(typeof(DecoratorRegistrationExtensionsTests).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        IEventHandler<TestEvent> handler = provider.GetRequiredService<IEventHandler<TestEvent>>();
        var probe = provider.GetRequiredService<InvocationProbe>();

        Assert.Contains("EventHandlerLoggingDecorator", handler.GetType().Name, StringComparison.Ordinal);

        await handler.HandleAsync(new TestEvent());

        Assert.Equal(1, probe.EventInvocations);
    }

    [Fact]
    public void AddCommandHandlersLogging_WithAssemblyWithoutHandlers_LeavesRegistrationUntouched()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<InvocationProbe>();
        services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        builder.AddCommandHandlersLogging(typeof(string).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        ICommandHandler<TestCommand> handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();

        Assert.Equal(typeof(TestCommandHandler), handler.GetType());
    }

    public sealed class InvocationProbe
    {
        public int CommandInvocations { get; private set; }
        public int EventInvocations { get; private set; }

        public void OnCommand() => CommandInvocations++;

        public void OnEvent() => EventInvocations++;
    }

    public sealed record TestCommand : ICommand;

    public sealed class TestCommandHandler(InvocationProbe probe) : ICommandHandler<TestCommand>
    {
        private readonly InvocationProbe _probe = probe;

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            _probe.OnCommand();
            return Task.CompletedTask;
        }
    }

    public sealed record TestEvent : IEvent;

    public sealed class TestEventHandler(InvocationProbe probe) : IEventHandler<TestEvent>
    {
        private readonly InvocationProbe _probe = probe;

        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            _probe.OnEvent();
            return Task.CompletedTask;
        }
    }
}
