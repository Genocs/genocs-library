using System.Text.Json;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Genocs.Common.CQRS.Events;
using Genocs.Messaging.AzureServiceBus.Configurations;
using Genocs.Messaging.AzureServiceBus.Topics.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Messaging.AzureServiceBus.Topics;

/// <summary>
/// Azure Service Bus Topic implementation using Azure.Messaging.ServiceBus SDK.
/// </summary>
public class AzureServiceBusTopic : IAzureServiceBusTopic, IHostedService, IAsyncDisposable
{
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";
    private const string EVENTSUFFIX = "Event";
    private static readonly ActivitySource ActivitySource = new("Genocs.Messaging.AzureServiceBus");

    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor? _processor;
    private readonly AzureServiceBusTopicOptions _options;
    private readonly ILogger<AzureServiceBusTopic> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
    private bool _isProcessorStarted;

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
        _handlers = [];

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
        string eventName = @event.GetType().Name.Replace(EVENTSUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        using var producerActivity = StartProducerActivity(message, eventName, "publish");
        InjectTraceContext(message.ApplicationProperties);

        try
        {
            await _sender.SendMessageAsync(message);
        }
        catch (Exception exception)
        {
            MarkActivityAsError(producerActivity, exception);
            throw;
        }
    }

    /// <summary>
    /// Publishes an event to the topic with custom application properties for filtering.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="filters">Application properties to attach to the message for subscription filtering.</param>
    public async Task PublishAsync(IEvent @event, Dictionary<string, object> filters)
    {
        string eventName = @event.GetType().Name.Replace(EVENTSUFFIX, string.Empty);
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

        using var producerActivity = StartProducerActivity(message, eventName, "publish");
        InjectTraceContext(message.ApplicationProperties);

        try
        {
            await _sender.SendMessageAsync(message);
        }
        catch (Exception exception)
        {
            MarkActivityAsError(producerActivity, exception);
            throw;
        }
    }

    /// <summary>
    /// Schedules an event to be published at a specified time.
    /// </summary>
    /// <param name="event">The event to schedule.</param>
    /// <param name="offset">The time at which the message should be enqueued.</param>
    public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset)
    {
        string eventName = @event.GetType().Name.Replace(EVENTSUFFIX, string.Empty);
        string jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = eventName,
        };

        using var producerActivity = StartProducerActivity(message, eventName, "schedule");
        InjectTraceContext(message.ApplicationProperties);

        try
        {
            await _sender.ScheduleMessageAsync(message, offset);
        }
        catch (Exception exception)
        {
            MarkActivityAsError(producerActivity, exception);
            throw;
        }
    }

    /// <summary>
    /// Schedules an event to be published at a specified time with custom application properties.
    /// </summary>
    /// <param name="event">The event to schedule.</param>
    /// <param name="offset">The time at which the message should be enqueued.</param>
    /// <param name="filters">Application properties to attach to the message for subscription filtering.</param>
    public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset, Dictionary<string, object> filters)
    {
        string eventName = @event.GetType().Name.Replace(EVENTSUFFIX, string.Empty);
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

        using var producerActivity = StartProducerActivity(message, eventName, "schedule");
        InjectTraceContext(message.ApplicationProperties);

        try
        {
            await _sender.ScheduleMessageAsync(message, offset);
        }
        catch (Exception exception)
        {
            MarkActivityAsError(producerActivity, exception);
            throw;
        }
    }

    /// <summary>
    /// Subscribes to events on the topic using the modern event handler contract.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <typeparam name="TH">The event handler type.</typeparam>
    public void SubscribeModern<T, TH>()
        where T : class, IEvent
        where TH : IEventHandler<T>
    {
        RegisterSubscription(typeof(T), typeof(TH), usesLegacyContract: false);
    }

    /// <summary>
    /// Subscribes to events on the topic with a specified handler.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <typeparam name="TH">The event handler type.</typeparam>
    /// <exception cref="ArgumentException">Thrown when the handler is already registered for the event.</exception>
    [Obsolete("Subscribe<T,TH>() uses legacy IEventHandlerLegacy<T>. Use SubscribeModern<T,TH>() with IEventHandler<T>. Legacy registration will be removed in a future major release.")]
    public void Subscribe<T, TH>()
        where T : IEvent
        where TH : IEventHandlerLegacy<T>
    {
        RegisterSubscription(typeof(T), typeof(TH), usesLegacyContract: true);
    }

    private void RegisterSubscription(Type eventType, Type handlerType, bool usesLegacyContract)
    {
        string key = eventType.Name;
        if (!_handlers.ContainsKey(key))
        {
            _handlers.Add(key, []);
        }

        if (_handlers[key].Any(s => s.HandlerType == handlerType))
        {
            throw new ArgumentException(
                $"Handler Type '{handlerType.Name}' already registered for '{key}'", nameof(handlerType));
        }

        _handlers[key].Add(SubscriptionInfo.Typed(eventType, handlerType, usesLegacyContract));
    }

    private void RegisterSubscriptionClientMessageHandler()
    {
        _processor!.ProcessMessageAsync += async (args) =>
        {
            // Subject must match Subscribe<T> key (typeof(T).Name); see PublishAsync Subject.
            string eventName = args.Message.Subject ?? string.Empty;
            using var processingActivity = StartConsumerActivity(args.Message, eventName);
            string messageData = args.Message.Body.ToString();

            // Complete the message so that it is not received again.
            if (await ProcessEvent(eventName, messageData, args.CancellationToken))
            {
                await args.CompleteMessageAsync(args.Message);
            }
        };

        _processor.ProcessErrorAsync += ExceptionReceivedHandler;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_processor is null || _isProcessorStarted)
        {
            return;
        }

        await _processor.StartProcessingAsync(cancellationToken);
        _isProcessorStarted = true;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is null || !_isProcessorStarted)
        {
            return;
        }

        await _processor.StopProcessingAsync(cancellationToken);
        _isProcessorStarted = false;
    }

    private static void InjectTraceContext(IDictionary<string, object> applicationProperties)
    {
        Activity? currentActivity = Activity.Current;

        if (!string.IsNullOrWhiteSpace(currentActivity?.Id) && !applicationProperties.ContainsKey(TraceParentHeader))
        {
            applicationProperties[TraceParentHeader] = currentActivity.Id;
        }

        if (!string.IsNullOrWhiteSpace(currentActivity?.TraceStateString) && !applicationProperties.ContainsKey(TraceStateHeader))
        {
            applicationProperties[TraceStateHeader] = currentActivity.TraceStateString;
        }
    }

    private Activity? StartProducerActivity(ServiceBusMessage message, string messageType, string operation)
    {
        var tags = new ActivityTagsCollection
        {
            ["messaging.system"] = "azureservicebus",
            ["messaging.operation"] = operation,
            ["messaging.destination.name"] = _options.TopicName,
            ["messaging.destination_kind"] = "topic",
            ["messaging.message.id"] = message.MessageId,
            ["genocs.message.type"] = messageType
        };

        return ActivitySource.StartActivity($"azureservicebus.{operation}", ActivityKind.Producer, default(ActivityContext), tags);
    }

    private Activity? StartConsumerActivity(ServiceBusReceivedMessage message, string eventName)
    {
        ActivityContext parentContext = default;
        TryExtractParentContext(message, out parentContext);

        var tags = new ActivityTagsCollection
        {
            ["messaging.system"] = "azureservicebus",
            ["messaging.operation"] = "process",
            ["messaging.destination.name"] = _options.TopicName,
            ["messaging.message.id"] = message.MessageId,
            ["messaging.conversation_id"] = message.CorrelationId,
            ["genocs.message.type"] = eventName
        };

        return ActivitySource.StartActivity("azureservicebus.process", ActivityKind.Consumer, parentContext, tags);
    }

    private static bool TryExtractParentContext(ServiceBusReceivedMessage message, out ActivityContext parentContext)
    {
        string? traceParent = TryGetApplicationProperty(message, TraceParentHeader);
        string? traceState = TryGetApplicationProperty(message, TraceStateHeader);

        if (string.IsNullOrWhiteSpace(traceParent))
        {
            parentContext = default;
            return false;
        }

        return ActivityContext.TryParse(traceParent, traceState, out parentContext);
    }

    private static string? TryGetApplicationProperty(ServiceBusReceivedMessage message, string propertyName)
    {
        if (!message.ApplicationProperties.TryGetValue(propertyName, out object? value) || value is null)
        {
            return null;
        }

        return value.ToString();
    }

    private static void MarkActivityAsError(Activity? activity, Exception exception)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().FullName);
        activity.SetTag("error.message", exception.Message);
    }

    private Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "ERROR handling message: {ErrorMessage} - Source: {ErrorSource}",
            args.Exception.Message,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    private async Task<bool> ProcessEvent(string eventName, string message, CancellationToken cancellationToken)
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
                        object? @event = JsonSerializer.Deserialize(message, subscription.EventType);
                        if (@event is null)
                        {
                            _logger.LogError("Failed to deserialize message for event '{EventName}'", eventName);
                            continue;
                        }

                        if (subscription.UsesLegacyContract)
                        {
                            var legacyType = typeof(IEventHandlerLegacy<>).MakeGenericType(subscription.EventType);
                            await (Task)legacyType.GetMethod(nameof(IEventHandlerLegacy<IEvent>.HandleEvent))!
                                .Invoke(handler, new[] { @event })!;
                        }
                        else
                        {
                            var modernType = typeof(IEventHandler<>).MakeGenericType(subscription.EventType);
                            await (Task)modernType.GetMethod(nameof(IEventHandler<IEvent>.HandleAsync))!
                                .Invoke(handler, new[] { @event, cancellationToken })!;
                        }
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
            if (_isProcessorStarted)
            {
                await StopAsync(CancellationToken.None);
            }

            await _processor.DisposeAsync();
        }

        await _sender.DisposeAsync();
        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
