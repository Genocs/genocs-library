using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Commons;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.CQRS;

public class DispatcherWiringIntegrationTests
{
    [Fact]
    public async Task ProjectFilterScanning_WiresHandlersAndDispatchers_EndToEnd()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DispatchIntegrationTracker>();

        services.AddHandlers("Genocs.Core.UnitTests");
        services.AddDispatchers();

        using ServiceProvider provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var tracker = provider.GetRequiredService<DispatchIntegrationTracker>();

        await dispatcher.SendAsync(new FilterCommand(7));
        await dispatcher.PublishAsync(new FilterEvent("evt-1"));
        int? result = await dispatcher.QueryAsync(new FilterQuery(11));

        Assert.True(tracker.CommandHandled);
        Assert.True(tracker.EventHandled);
        Assert.Equal("evt-1", tracker.EventName);
        Assert.Equal(11, result);
    }

    [Fact]
    public async Task ProjectFilterScanning_WithNoMatchingAssemblies_LeavesMissingHandlers()
    {
        var services = new ServiceCollection();

        services.AddHandlers("project-does-not-exist");
        services.AddDispatchers();

        using ServiceProvider provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(new FilterCommand(1)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.QueryAsync(new FilterQuery(1)));

        // Event publish should complete even with no handlers.
        await dispatcher.PublishAsync(new FilterEvent("no-handlers"));
    }

    [Fact]
    public async Task AppDomainScanningPath_WiresHandlersAndDispatchers_EndToEnd()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DispatchIntegrationTracker>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        Genocs.Core.CQRS.Commands.Extensions.AddCommandHandlers(builder);
        Genocs.Core.CQRS.Events.Extensions.AddEventHandlers(builder);
        Genocs.Core.CQRS.Queries.Extensions.AddQueryHandlers(builder);

        services.AddDispatchers();

        using ServiceProvider provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var tracker = provider.GetRequiredService<DispatchIntegrationTracker>();

        await dispatcher.SendAsync(new FilterCommand(21));
        await dispatcher.PublishAsync(new FilterEvent("evt-2"));
        int? result = await dispatcher.QueryAsync(new FilterQuery(31));

        Assert.True(tracker.CommandHandled);
        Assert.True(tracker.EventHandled);
        Assert.Equal("evt-2", tracker.EventName);
        Assert.Equal(31, result);
    }

    public sealed class DispatchIntegrationTracker
    {
        public bool CommandHandled { get; set; }

        public bool EventHandled { get; set; }

        public string EventName { get; set; }
    }

    public sealed record FilterCommand(int Value) : ICommand;

    public sealed class FilterCommandHandler : ICommandHandler<FilterCommand>
    {
        private readonly DispatchIntegrationTracker _tracker;

        public FilterCommandHandler(DispatchIntegrationTracker tracker)
            => _tracker = tracker;

        public Task HandleAsync(FilterCommand command, CancellationToken cancellationToken = default)
        {
            _tracker.CommandHandled = true;
            return Task.CompletedTask;
        }
    }

    public sealed record FilterEvent(string Name) : IEvent;

    public sealed class FilterEventHandler : IEventHandler<FilterEvent>
    {
        private readonly DispatchIntegrationTracker _tracker;

        public FilterEventHandler(DispatchIntegrationTracker tracker)
            => _tracker = tracker;

        public Task HandleAsync(FilterEvent @event, CancellationToken cancellationToken = default)
        {
            _tracker.EventHandled = true;
            _tracker.EventName = @event.Name;
            return Task.CompletedTask;
        }
    }

    public sealed record FilterQuery(int Value) : IQuery<int>;

    public sealed class FilterQueryHandler : IQueryHandler<FilterQuery, int>
    {
        public Task<int> HandleAsync(FilterQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(query.Value);
    }
}
