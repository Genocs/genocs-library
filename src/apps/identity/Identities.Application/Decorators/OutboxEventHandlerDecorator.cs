using Genocs.Common.Types;
using Genocs.Core.CQRS.Events;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.Configurations;

namespace Genocs.Identities.Application.Decorators;

[Decorator]
internal sealed class OutboxEventHandlerDecorator<TEvent> : IEventHandler<TEvent>
    where TEvent : class, IEvent
{
    private readonly IEventHandler<TEvent> _handler;
    private readonly IMessageOutbox _outbox;
    private readonly string _messageId;
    private readonly bool _enabled;

    public OutboxEventHandlerDecorator(
                                        IEventHandler<TEvent> handler,
                                        IMessageOutbox outbox,
                                        OutboxOptions outboxOptions,
                                        IMessagePropertiesAccessor messagePropertiesAccessor)
    {
        _handler = handler;
        _outbox = outbox;
        _enabled = outboxOptions.Enabled;

        var messageProperties = messagePropertiesAccessor.MessageProperties;
        _messageId = string.IsNullOrWhiteSpace(messageProperties?.MessageId)
            ? Guid.NewGuid().ToString("N")
            : messageProperties.MessageId;
    }

    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        => _enabled
            ? _outbox.HandleAsync(_messageId, () => _handler.HandleAsync(@event))
            : _handler.HandleAsync(@event);

}