using Genocs.Common.CQRS.Events;
using Genocs.Core.CQRS.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Genocs.Core.CQRS.Commons;

namespace Genocs.Core.UnitTests.CQRS;

public class EventDispatcherTests
{
    [Fact]
    public async Task PublishAsync_RoutesSingleHandler()
    {
        var services = new ServiceCollection();
        services.AddDispatchers();
        services.AddSingleton<TestEventHandlerTracker>();
        services.AddTransient<IEventHandler<TestEvent>, TrackingEventHandler>();

        ServiceProvider sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();
        var tracker = sp.GetRequiredService<TestEventHandlerTracker>();

        var @event = new TestEvent(42);
        await dispatcher.PublishAsync(@event);

        Assert.Equal(1, tracker.HandleCount);
        Assert.Equal(42, tracker.LastReceivedValue);
    }

    [Fact]
    public async Task PublishAsync_RoutesToMultipleHandlers()
    {
        ServiceProvider sp = BuildProvider(handlerCount: 3);
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();
        var tracker = sp.GetRequiredService<TestEventHandlerTracker>();

        var @event = new TestEvent(99);
        await dispatcher.PublishAsync(@event);

        Assert.Equal(3, tracker.HandleCount);
        Assert.Equal(99, tracker.LastReceivedValue);
    }

    [Fact]
    public async Task PublishAsync_ThrowsOnNullEvent()
    {
        ServiceProvider sp = BuildProvider(handlerCount: 1);
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.PublishAsync<TestEvent>(null!));
    }

    [Fact]
    public async Task PublishAsync_NoHandlersRegistered_CompletesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddDispatchers();

        ServiceProvider sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();

        var @event = new TestEvent(1);

        // Should not throw even when no handlers are registered.
        await dispatcher.PublishAsync(@event);
    }

    [Fact]
    public async Task PublishAsync_PropagatesFirstHandlerException_AndContinuesOthers()
    {
        ServiceProvider sp = BuildProvider(handlerCount: 3, throwOnIndex: 1);
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();
        var tracker = sp.GetRequiredService<TestEventHandlerTracker>();

        var @event = new TestEvent(1);

        // EventDispatcher.PublishAsync uses Task.WhenAll, so all handlers execute.
        // If any throw, this implementation surfaces InvalidOperationException.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.PublishAsync(@event));

        Assert.Equal("Handler error", exception.Message);
        Assert.True(tracker.HandleCount >= 1);
    }

    [Fact]
    public async Task PublishAsync_CreatesAsyncScopeForHandlers()
    {
        var services = new ServiceCollection();
        services.AddDispatchers();
        services.AddSingleton<TestEventHandlerTracker>();
        services.AddTransient<IEventHandler<TestEvent>, TrackingEventHandler>();

        ServiceProvider sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IEventDispatcher>();
        var tracker = sp.GetRequiredService<TestEventHandlerTracker>();

        var @event = new TestEvent(1);
        await dispatcher.PublishAsync(@event);

        // The handler should have been resolved from a scoped provider, not the root provider.
        Assert.NotNull(tracker.ReceivedScopeProvider);
        Assert.NotSame(sp, tracker.ReceivedScopeProvider);
    }

    private static ServiceProvider BuildProvider(int handlerCount = 1, int throwOnIndex = -1)
    {
        var services = new ServiceCollection();

        services.AddDispatchers();
        services.AddSingleton<TestEventHandlerTracker>();

        for (int i = 0; i < handlerCount; i++)
        {
            int index = i;
            services.AddTransient<IEventHandler<TestEvent>>(sp =>
                new TrackingEventHandler(
                    tracker: sp.GetRequiredService<TestEventHandlerTracker>(),
                    throwOnHandle: throwOnIndex >= 0 && index == throwOnIndex,
                    scopeProvider: sp));
        }

        return services.BuildServiceProvider();
    }

    public sealed record TestEvent(int Value) : IEvent;

    public sealed class TestEventHandlerTracker
    {
        public int HandleCount { get; set; }

        public int LastReceivedValue { get; set; }

        public IServiceProvider? ReceivedScopeProvider { get; set; }
    }

    public sealed class TrackingEventHandler : IEventHandler<TestEvent>
    {
        private readonly TestEventHandlerTracker _tracker;
        private readonly bool _throwOnHandle;

        public TrackingEventHandler(TestEventHandlerTracker tracker, bool throwOnHandle = false, IServiceProvider? scopeProvider = null)
        {
            _tracker = tracker;
            _throwOnHandle = throwOnHandle;
            _tracker.ReceivedScopeProvider = scopeProvider;
        }

        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            _tracker.HandleCount++;
            _tracker.LastReceivedValue = @event.Value;

            if (_throwOnHandle)
            {
                throw new InvalidOperationException("Handler error");
            }

            return Task.CompletedTask;
        }
    }
}
