using Genocs.Core.CQRS.Commands;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.SignalR.WebApi.Commands;
using Genocs.SignalR.WebApi.Event;
using OpenTracing;

namespace Genocs.Orders.WebApi.Commands.Handlers;

public class PublishNotificationHandler : ICommandHandler<PublishNotification>
{
    private readonly IBusPublisher _publisher;
    private readonly IMessageOutbox _outbox;
    private readonly ILogger<PublishNotificationHandler> _logger;
    private readonly ITracer _tracer;

    public PublishNotificationHandler(IBusPublisher publisher,
                                      IMessageOutbox outbox,
                                      ITracer tracer,
                                      ILogger<PublishNotificationHandler> logger)
    {
        _publisher = publisher;
        _outbox = outbox;
        _tracer = tracer;
        _logger = logger;
    }

    public async Task HandleAsync(PublishNotification command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Created a notification with id: {command.NotificationId}, customer: {command.CustomerId}.");
        var spanContext = _tracer.ActiveSpan?.Context.ToString();
        var @event = new NotificationPosted(command.NotificationId);
        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext);
    }
}