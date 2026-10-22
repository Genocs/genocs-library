using Genocs.Common.CQRS.Commands;
using Genocs.Core.CQRS.Commands;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Genocs.Core.CQRS.Commons;

namespace Genocs.Core.UnitTests.CQRS;

public class CommandDispatcherTests
{
    [Fact]
    public async Task SendAsync_RoutesToRegisteredHandler()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();
        var tracker = sp.GetRequiredService<TestCommandHandlerTracker>();

        var command = new TestCommand(42);
        await dispatcher.SendAsync(command);

        Assert.True(tracker.HandleCalled);
        Assert.Equal(42, tracker.ReceivedValue);
    }

    [Fact]
    public async Task SendAsync_ThrowsOnNullCommand()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dispatcher.SendAsync<TestCommand>(null!));
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenHandlerNotRegistered()
    {
        ServiceProvider sp = BuildProvider(includeHandler: false);
        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();

        var command = new TestCommand(1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_PropagatesHandlerException()
    {
        ServiceProvider sp = BuildProvider(throwOnHandle: true);
        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();

        var command = new TestCommand(1);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.SendAsync(command));

        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public async Task SendAsync_CreatesAsyncScopeForHandler()
    {
        ServiceProvider sp = BuildProvider(trackScope: true);
        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();
        var tracker = sp.GetRequiredService<TestCommandHandlerTracker>();

        var command = new TestCommand(1);
        await dispatcher.SendAsync(command);

        Assert.True(tracker.ScopedProviderUsed);
        Assert.NotNull(tracker.ReceivedScopeProvider);
        Assert.NotSame(sp, tracker.ReceivedScopeProvider);
    }

    private static ServiceProvider BuildProvider(bool includeHandler = true, bool throwOnHandle = false, bool trackScope = false)
    {
        var services = new ServiceCollection();

        services.AddDispatchers();
        services.AddSingleton<TestCommandHandlerTracker>();

        if (trackScope)
        {
            services.AddTransient<ICommandHandler<TestCommand>, TrackingScopeCommandHandler>();
        }
        else if (includeHandler)
        {
            services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>(sp =>
                new TestCommandHandler(
                    sp.GetRequiredService<TestCommandHandlerTracker>(),
                    sp,
                    throwOnHandle));
        }

        return services.BuildServiceProvider();
    }

    public sealed record TestCommand(int Value) : ICommand;

    public sealed class TestCommandHandlerTracker
    {
        public bool HandleCalled { get; set; }

        public int ReceivedValue { get; set; }

        public bool ScopedProviderUsed { get; set; }

        public IServiceProvider? ReceivedScopeProvider { get; set; }
    }

    public sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly TestCommandHandlerTracker _tracker;
        private readonly IServiceProvider _scopeProvider;
        private readonly bool _throwError;

        public TestCommandHandler(TestCommandHandlerTracker tracker, IServiceProvider scopeProvider, bool throwError = false)
        {
            _tracker = tracker;
            _scopeProvider = scopeProvider;
            _throwError = throwError;
        }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _tracker.HandleCalled = true;
            _tracker.ReceivedValue = command.Value;
            _tracker.ReceivedScopeProvider = _scopeProvider;

            if (_throwError)
            {
                throw new InvalidOperationException("Handler error");
            }

            return Task.CompletedTask;
        }
    }

    public sealed class TrackingScopeCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly TestCommandHandlerTracker _tracker;
        private readonly IServiceProvider _scopeProvider;

        public TrackingScopeCommandHandler(TestCommandHandlerTracker tracker, IServiceProvider scopeProvider)
        {
            _tracker = tracker;
            _scopeProvider = scopeProvider;
        }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            _tracker.ScopedProviderUsed = true;
            _tracker.ReceivedScopeProvider = _scopeProvider;
            return Task.CompletedTask;
        }
    }
}

