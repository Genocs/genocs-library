using Genocs.Core.CQRS.Commands;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.SignalR.WebApi.Events;
using Genocs.SignalR.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using OpenTracing;

namespace Genocs.SignalR.WebApi.Commands.Handlers;

public class PublishNotificationHandler : ICommandHandler<PublishNotification>
{
    private readonly IBusPublisher _publisher;
    private readonly IMessageOutbox _outbox;
    private readonly ILogger<PublishNotificationHandler> _logger;
    private readonly ITracer _tracer;
    private readonly IHubContext<GenocsHub> _hub;

    public PublishNotificationHandler(
                                      IBusPublisher publisher,
                                      IMessageOutbox outbox,
                                      ITracer tracer,
                                      ILogger<PublishNotificationHandler> logger,
                                      IHubContext<GenocsHub> hub)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
    }

    public async Task HandleAsync(PublishNotification command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Created a notification with id: {command.NotificationId}, customer: {command.CustomerId}.");
        string? spanContext = _tracer.ActiveSpan?.Context.ToString();
        var @event = new NotificationPosted(command.NotificationId);

        // Send the notification
        await _hub.Clients.All.SendAsync("PublishNotification", @event);

        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext);
    }
}