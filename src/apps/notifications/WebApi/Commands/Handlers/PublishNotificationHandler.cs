using Genocs.Common.CQRS.Commands;
using Genocs.Messaging;
using Genocs.Messaging.Outbox;
using Genocs.Notifications.WebApi.Events;
using Genocs.Notifications.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Genocs.Notifications.WebApi.Commands.Handlers;

public class PublishNotificationHandler : ICommandHandler<PublishNotification>
{
    private readonly IBusPublisher _publisher;
    private readonly IMessageOutbox _outbox;
    private readonly ILogger<PublishNotificationHandler> _logger;

    private readonly IHubContext<GenocsHub> _hub;

    public PublishNotificationHandler(
                                      IBusPublisher publisher,
                                      IMessageOutbox outbox,
                                      ILogger<PublishNotificationHandler> logger,
                                      IHubContext<GenocsHub> hub)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
    }

    public async Task HandleAsync(PublishNotification command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Created a notification with id: {command.NotificationId}, customer: {command.CustomerId}.");
        string? spanContext = System.Diagnostics.Activity.Current?.Id;
        var @event = new NotificationPosted(command.NotificationId);

        // Send the notification
        await _hub.Clients.All.SendAsync("PublishNotification", @event, cancellationToken);

        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext, cancellationToken: cancellationToken);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext);
    }
}