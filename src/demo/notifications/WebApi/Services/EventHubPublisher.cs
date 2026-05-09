using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Genocs.Notifications.WebApi.Configurations;
using Genocs.Notifications.WebApi.Messages.Events;
using Microsoft.Extensions.Options;

namespace Genocs.Notifications.WebApi.Services;

public class EventHubPublisher(IOptions<EventHubOptions> options, ILogger<EventHubPublisher> logger) : IEventHubPublisher, IAsyncDisposable
{
    private readonly EventHubOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<EventHubPublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly EventHubProducerClient? _producerClient = CreateProducerClient(options?.Value, logger);

    public async Task PublishOrderCreatedAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        if (_producerClient is null)
        {
            return;
        }

        string payload = JsonSerializer.Serialize(@event);
        var eventData = new EventData(BinaryData.FromString(payload))
        {
            ContentType = "application/json",
            MessageId = @event.OrderId.ToString("N")
        };

        using EventDataBatch batch = await _producerClient.CreateBatchAsync(cancellationToken);
        if (!batch.TryAdd(eventData))
        {
            throw new InvalidOperationException($"Unable to add order '{@event.OrderId}' to the Event Hub batch.");
        }

        await _producerClient.SendAsync(batch, cancellationToken);
        _logger.LogInformation("Forwarded order '{OrderId}' to Event Hub '{EventHubName}'.", @event.OrderId, _options.Name);
    }

    public async ValueTask DisposeAsync()
    {
        if (_producerClient is not null)
        {
            await _producerClient.DisposeAsync();
        }
    }

    private static EventHubProducerClient? CreateProducerClient(EventHubOptions? options, ILogger<EventHubPublisher> logger)
    {
        if (options is null || !options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString) || string.IsNullOrWhiteSpace(options.Name))
        {
            logger.LogWarning("Event Hub is enabled but configuration is incomplete. Forwarding is disabled.");
            return null;
        }

        return new EventHubProducerClient(options.ConnectionString, options.Name);
    }
}