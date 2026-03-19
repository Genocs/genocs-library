using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;

namespace Genocs.Messaging.CQRS.Dispatchers;

internal sealed class ServiceBusMessageDispatcher : ICommandDispatcher, IEventDispatcher
{
    private readonly IBusPublisher _busPublisher;
    private readonly ICorrelationContextAccessor _accessor;

    public ServiceBusMessageDispatcher(IBusPublisher busPublisher, ICorrelationContextAccessor accessor)
    {
        _busPublisher = busPublisher;
        _accessor = accessor;
    }

    public Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand
        => _busPublisher.SendAsync(command, _accessor.CorrelationContext);

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent
        => _busPublisher.PublishAsync(@event, _accessor.CorrelationContext);
}