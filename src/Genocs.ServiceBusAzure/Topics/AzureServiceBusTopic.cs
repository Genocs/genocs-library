using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Genocs.Common.CQRS.Events;
using Genocs.ServiceBusAzure.Configurations;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.ServiceBusAzure.Topics;

/// <summary>
/// Azure Service Bus Topic implementation using Azure.Messaging.ServiceBus SDK.
/// </summary>
public class AzureServiceBusTopic : IAzureServiceBusTopic, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor? _processor;
    private readonly AzureServiceBusTopicOptions _options;
    private readonly ILogger<AzureServiceBusTopic> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const string EVENT_SUFFIX = "Event";
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
    private readonly List<Type> _eventTypes;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureServiceBusTopic"/> using <see cref="IOptions{T}"/>.
    /// </summary>
    /// <param name="options">The topic configuration options.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AzureServiceBusTopic(
        IOptions<AzureServiceBusTopicOptions> options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusTopic> logger)
        : this(options?.Value ?? throw new ArgumentNullException(nameof(options)), serviceProvider, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AzureServiceBusTopic"/> using direct options.
    /// </summary>
    /// <param name="options">The topic configuration options.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AzureServiceBusTopic(
        AzureServiceBusTopicOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusTopic> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _client = new ServiceBusClient(_options.ConnectionString);
        _sender = _client.CreateSender(_options.TopicName);
        _handlers = new Dictionary<string, List<SubscriptionInfo>>();
        _eventTypes = new List<Type>();

        if (!string.IsNullOrEmpty(_options.SubscriptionName))
        {
            _processor = _client.CreateProcessor(_options.TopicName, _options.SubscriptionName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _options.MaxConcurrentCalls,
                PrefetchCount = _options.PrefetchCount,
                ReceiveMode = _options.ReceiveMode,
                AutoCompleteMessages = false
            });

            RegisterSubscriptionClientMessageHandler();
        }
    }

    /// <summary>
    /// Publishes an event to the topic.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    public async Task PublishAsync(IEvent @event)
    {
        string eventName = @event.GetType().Name.Replace(EVENT_SUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        await _sender.SendMessageAsync(message);
    }

    /// <summary>
    /// Publishes an event to the topic with custom application properties for filtering.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="filters">Application properties to attach to the message for subscription filtering.</param>
    public async Task PublishAsync(IEvent @event, Dictionary<string, object> filters)
    {
        string eventName = @event.GetType().Name.Replace(EVENT_SUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        foreach (KeyValuePair<string, object> filter in filters)
        {
            message.ApplicationProperties.Add(filter);
        }

        await _sender.SendMessageAsync(message);
    }

    /// <summary>
    /// Schedules an event to be published at a specified time.
    /// </summary>
    /// <param name="event">The event to schedule.</param>
    /// <param name="offset">The time at which the message should be enqueued.</param>
    public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset)
    {
        string eventName = @event.GetType().Name.Replace(EVENT_SUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        await _sender.ScheduleMessageAsync(message, offset);
    }

    /// <summary>
    /// Schedules an event to be published at a specified time with custom application properties.
    /// </summary>
    /// <param name="event">The event to schedule.</param>
    /// <param name="offset">The time at which the message should be enqueued.</param>
    /// <param name="filters">Application properties to attach to the message for subscription filtering.</param>
    public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset, Dictionary<string, object> filters)
    {
        string eventName = @event.GetType().Name.Replace(EVENT_SUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        foreach (KeyValuePair<string, object> filter in filters)
        {
            message.ApplicationProperties.Add(filter);
        }

        await _sender.ScheduleMessageAsync(message, offset);
    }

    /// <summary>
    /// Subscribes to events on the topic with a specified handler.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <typeparam name="TH">The event handler type.</typeparam>
    /// <exception cref="ArgumentException">Thrown when the handler is already registered for the event.</exception>
    public void Subscribe<T, TH>()
        where T : IEvent
        where TH : IEventHandlerLegacy<T>
    {
        string key = typeof(T).Name;
        if (!_handlers.ContainsKey(key))
        {
            _handlers.Add(key, []);
        }

        Type handlerType = typeof(TH);

        if (_handlers[key].Any(s => s.HandlerType == handlerType))
        {
            throw new ArgumentException(
                $"Handler Type '{typeof(TH).Name}' already registered for '{key}'", nameof(handlerType));
        }

        if (!_eventTypes.Contains(typeof(T)))
        {
            _eventTypes.Add(typeof(T));
        }

        _handlers[key].Add(SubscriptionInfo.Typed(handlerType));
    }

    private void RegisterSubscriptionClientMessageHandler()
    {
        _processor!.ProcessMessageAsync += async (args) =>
        {
            string eventName = $"{args.Message.Subject}{EVENT_SUFFIX}";
            string messageData = args.Message.Body.ToString();

            // Complete the message so that it is not received again.
            if (await ProcessEvent(eventName, messageData))
            {
                await args.CompleteMessageAsync(args.Message);
            }
        };

        _processor.ProcessErrorAsync += ExceptionReceivedHandler;

        _processor.StartProcessingAsync().GetAwaiter().GetResult();
    }

    private Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "ERROR handling message: {ErrorMessage} - Source: {ErrorSource}",
            args.Exception.Message, args.ErrorSource);
        return Task.CompletedTask;
    }

    private async Task<bool> ProcessEvent(string eventName, string message)
    {
        bool processed = false;
        if (_handlers.ContainsKey(eventName))
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var subscriptions = _handlers[eventName];

                foreach (var subscription in subscriptions)
                {
                    object handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                    if (handler != null)
                    {
                        var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                        object? command = JsonSerializer.Deserialize(message, eventType!);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType!);
                        await (Task)concreteType.GetMethod("HandleEvent")!.Invoke(handler, new object[] { command! })!;
                    }
                }
            }

            processed = true;
        }
        else
        {
            _logger.LogError("Event '{EventName}' does not have any handlers. Check whether Subscribe is set", eventName);
        }

        return processed;
    }

    /// <summary>
    /// Disposes the Service Bus client, sender, and processor.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync();
            await _processor.DisposeAsync();
        }

        await _sender.DisposeAsync();
        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
