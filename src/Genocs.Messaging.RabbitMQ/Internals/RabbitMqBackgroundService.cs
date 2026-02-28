using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Genocs.Core.Extensions;
using Genocs.Messaging.RabbitMQ.Plugins;
using Genocs.Messaging.RabbitMQ.Subscribers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ.Internals;

internal sealed class RabbitMqBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        WriteIndented = true
    };

    private readonly ConcurrentDictionary<string, IChannel> _channels = new();
    private readonly EmptyExceptionToMessageMapper _exceptionMapper = new();
    private readonly EmptyExceptionToFailedMessageMapper _exceptionFailedMapper = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _consumerConnection;
    private readonly IConnection _producerConnection;
    private readonly MessageSubscribersChannel _messageSubscribersChannel;
    private readonly IBusPublisher _publisher;
    private readonly IRabbitMQSerializer _rabbitMqSerializer;
    private readonly IConventionsProvider _conventionsProvider;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger _logger;
    private readonly IRabbitMqPluginsExecutor _pluginsExecutor;
    private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
    private readonly IExceptionToFailedMessageMapper _exceptionToFailedMessageMapper;
    private readonly int _retries;
    private readonly int _retryInterval;
    private readonly bool _loggerEnabled;
    private readonly bool _logMessagePayload;
    private readonly RabbitMQOptions _options;
    private readonly RabbitMQOptions.QosOptions _qosOptions;
    private readonly bool _requeueFailedMessages;

    public RabbitMqBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _consumerConnection = serviceProvider.GetRequiredService<ConsumerConnection>().Connection;
        _producerConnection = serviceProvider.GetRequiredService<ProducerConnection>().Connection;
        _messageSubscribersChannel = serviceProvider.GetRequiredService<MessageSubscribersChannel>();
        _publisher = _serviceProvider.GetRequiredService<IBusPublisher>();
        _rabbitMqSerializer = _serviceProvider.GetRequiredService<IRabbitMQSerializer>();
        _conventionsProvider = _serviceProvider.GetRequiredService<IConventionsProvider>();
        _contextProvider = _serviceProvider.GetRequiredService<IContextProvider>();
        _logger = _serviceProvider.GetRequiredService<ILogger<RabbitMQSubscriber>>();
        _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ?? _exceptionMapper;
        _exceptionToFailedMessageMapper = _serviceProvider.GetService<IExceptionToFailedMessageMapper>() ?? _exceptionFailedMapper;
        _pluginsExecutor = _serviceProvider.GetRequiredService<IRabbitMqPluginsExecutor>();
        _options = _serviceProvider.GetRequiredService<RabbitMQOptions>();
        _loggerEnabled = _options.Logger?.Enabled ?? false;
        _logMessagePayload = _options.Logger?.LogMessagePayload ?? false;
        _retries = _options.Retries >= 0 ? _options.Retries : 3;
        _retryInterval = _options.RetryInterval > 0 ? _options.RetryInterval : 2;
        _qosOptions = _options.Qos ?? new RabbitMQOptions.QosOptions();
        _requeueFailedMessages = _options.RequeueFailedMessages;
        if (_qosOptions.PrefetchCount < 1)
        {
            _qosOptions.PrefetchCount = 1;
        }

        if (!_loggerEnabled || _options.Logger?.LogConnectionStatus is not true)
        {
            return;
        }

        _consumerConnection.CallbackExceptionAsync += ConnectionOnCallbackExceptionAsync;
        _consumerConnection.ConnectionShutdownAsync += ConnectionOnConnectionShutdownAsync;
        _consumerConnection.ConnectionBlockedAsync += ConnectionOnConnectionBlockedAsync;
        _consumerConnection.ConnectionUnblockedAsync += ConnectionOnConnectionUnblockedAsync;

        _producerConnection.CallbackExceptionAsync += ConnectionOnCallbackExceptionAsync;
        _producerConnection.ConnectionShutdownAsync += ConnectionOnConnectionShutdownAsync;
        _producerConnection.ConnectionBlockedAsync += ConnectionOnConnectionBlockedAsync;
        _producerConnection.ConnectionUnblockedAsync += ConnectionOnConnectionUnblockedAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var messageSubscriber in _messageSubscribersChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                switch (messageSubscriber.Action)
                {
                    case MessageSubscriberAction.Subscribe:
                        await SubscribeAsync(messageSubscriber);
                        break;
                    case MessageSubscriberAction.Unsubscribe:
                        Unsubscribe(messageSubscriber);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown message subscriber action type.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"There was an error during RabbitMQ action: '{messageSubscriber.Action}'.");
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    private async Task SubscribeAsync(IMessageSubscriber messageSubscriber)
    {
        var conventions = _conventionsProvider.Get(messageSubscriber.Type);
        string channelKey = GetChannelKey(conventions);
        if (_channels.ContainsKey(channelKey))
        {
            return;
        }

        var channel = await _consumerConnection.CreateChannelAsync();
        string channelInfoLog = $"exchange: '{conventions.Exchange}', queue: '{conventions.Queue}', " +
                             $"routing key: '{conventions.RoutingKey}'";

        if (!_channels.TryAdd(channelKey, channel))
        {
            _logger.LogError($"Couldn't add the channel for {channelInfoLog}.");
            channel.Dispose();
            return;
        }

        _logger.LogTrace($"Added the channel: {channel.ChannelNumber} for {channelInfoLog}.");

        bool declare = _options.Queue?.Declare ?? true;
        bool durable = _options.Queue?.Durable ?? true;
        bool exclusive = _options.Queue?.Exclusive ?? false;
        bool autoDelete = _options.Queue?.AutoDelete ?? false;

        var deadLetterEnabled = _options.DeadLetter?.Enabled is true;
        var deadLetterExchange = deadLetterEnabled
            ? $"{_options.DeadLetter.Prefix}{_options.Exchange.Name}{_options.DeadLetter.Suffix}"
            : string.Empty;
        var deadLetterQueue = deadLetterEnabled
            ? $"{_options.DeadLetter.Prefix}{conventions.Queue}{_options.DeadLetter.Suffix}"
            : string.Empty;

        if (declare)
        {
            if (_loggerEnabled)
            {
                _logger.LogInformation($"Declaring a queue: '{conventions.Queue}' with routing key: " +
                                       $"'{conventions.RoutingKey}' for an exchange: '{conventions.Exchange}'.");
            }

            var queueArguments = deadLetterEnabled
                ? new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", deadLetterExchange},
                    {"x-dead-letter-routing-key", deadLetterQueue},
                }
                : new Dictionary<string, object>();

            await channel.QueueDeclareAsync(conventions.Queue, durable, exclusive, autoDelete, queueArguments);
        }

        await channel.QueueBindAsync(conventions.Queue, conventions.Exchange, conventions.RoutingKey);
        await channel.BasicQosAsync(_qosOptions.PrefetchSize, _qosOptions.PrefetchCount, _qosOptions.Global);

        if (_options.DeadLetter?.Enabled is true)
        {
            if (_options.DeadLetter.Declare)
            {
                var ttl = _options.DeadLetter.Ttl.HasValue
                    ? _options.DeadLetter.Ttl <= 0 ? 86400000 : _options.DeadLetter.Ttl
                    : null;

                var deadLetterArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", conventions.Exchange },
                    { "x-dead-letter-routing-key", conventions.Queue }
                };

                if (ttl.HasValue)
                {
                    deadLetterArgs["x-message-ttl"] = ttl.Value;
                }

                _logger.LogInformation($"Declaring a dead letter queue: '{deadLetterQueue}' " +
                                       $"for an exchange: '{deadLetterExchange}'{(ttl.HasValue ? $", message TTL: {ttl} ms." : ".")}");

                await channel.QueueDeclareAsync(
                                                deadLetterQueue,
                                                _options.DeadLetter.Durable,
                                                _options.DeadLetter.Exclusive,
                                                _options.DeadLetter.AutoDelete,
                                                deadLetterArgs);
            }

            await channel.QueueBindAsync(deadLetterQueue, deadLetterExchange, deadLetterQueue);
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var messageId = args.BasicProperties.MessageId;
                var correlationId = args.BasicProperties.CorrelationId;
                var timestamp = args.BasicProperties.Timestamp.UnixTime;
                var message = _rabbitMqSerializer.Deserialize(args.Body.Span, messageSubscriber.Type);

                if (_loggerEnabled)
                {
                    var messagePayload = _logMessagePayload ? Encoding.UTF8.GetString(args.Body.Span) : string.Empty;
                    _logger.LogInformation(
                        "Received a message with ID: '{MessageId}', " +
                                           "Correlation ID: '{CorrelationId}', timestamp: {Timestamp}, " +
                                           "queue: {Queue}, routing key: {RoutingKey}, exchange: {Exchange}, payload: {MessagePayload}",
                        messageId, correlationId, timestamp, conventions.Queue, conventions.RoutingKey, conventions.Exchange, messagePayload);
                }

                var correlationContext = BuildCorrelationContext(scope, args);

                Task Next(object m, object ctx, BasicDeliverEventArgs a)
                    => TryHandleAsync(channel, m, messageId, correlationId, ctx, a,
                        messageSubscriber.Handle, deadLetterEnabled);

                await _pluginsExecutor.ExecuteAsync(Next, message, correlationContext, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await channel.BasicNackAsync(args.DeliveryTag, false, _requeueFailedMessages);
                await Task.Yield();
            }
        };

        await channel.BasicConsumeAsync(conventions.Queue, false, consumer);
    }

    private object BuildCorrelationContext(IServiceScope scope, BasicDeliverEventArgs args)
    {
        var messagePropertiesAccessor = scope.ServiceProvider.GetRequiredService<IMessagePropertiesAccessor>();
        messagePropertiesAccessor.MessageProperties = new MessageProperties
        {
            MessageId = args.BasicProperties.MessageId,
            CorrelationId = args.BasicProperties.CorrelationId,
            Timestamp = args.BasicProperties.Timestamp.UnixTime,
            Headers = args.BasicProperties.Headers
        };
        var correlationContextAccessor = scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
        var correlationContext = _contextProvider.Get(args.BasicProperties.Headers);
        correlationContextAccessor.CorrelationContext = correlationContext;

        return correlationContext;
    }

    private async Task TryHandleAsync(
                                        IChannel channel,
                                        object message,
                                        string messageId,
                                        string correlationId,
                                        object messageContext,
                                        BasicDeliverEventArgs args,
                                        Func<IServiceProvider, object, object, Task> handle,
                                        bool deadLetterEnabled)
    {
        int currentRetry = 0;
        string? messageName = message.GetType().Name.Underscore();
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(_retries, _ => TimeSpan.FromSeconds(_retryInterval));

        await retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (_loggerEnabled)
                {
                    _logger.LogInformation("Handling a message: {MessageName} with ID: {MessageId}, " +
                                           "Correlation ID: {CorrelationId}, retry: {MessageRetry}",
                        messageName, messageId, correlationId, currentRetry);
                }

                if (_options.MessageProcessingTimeout.HasValue)
                {
                    var task = handle(_serviceProvider, message, messageContext);
                    var result = await Task.WhenAny(task, Task.Delay(_options.MessageProcessingTimeout.Value));
                    if (result != task)
                    {
                        throw new RabbitMqMessageProcessingTimeoutException(messageId, correlationId);
                    }
                }
                else
                {
                    await handle(_serviceProvider, message, messageContext);
                }

                channel.BasicAckAsync(args.DeliveryTag, false);
                await Task.Yield();

                if (_loggerEnabled)
                {
                    _logger.LogInformation(
                        "Handled a message: {MessageName} with ID: {MessageId}, " +
                        "Correlation ID: {CorrelationId}, retry: {MessageRetry}",
                        messageName,
                        messageId,
                        correlationId,
                        currentRetry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex is RabbitMqMessageProcessingTimeoutException)
                {
                    channel.BasicNackAsync(args.DeliveryTag, false, _requeueFailedMessages);
                    await Task.Yield();
                    return;
                }

                currentRetry++;
                bool hasNextRetry = currentRetry <= _retries;

                var failedMessage = _exceptionToFailedMessageMapper.Map(ex, message);
                if (failedMessage is null)
                {
                    // This is a fallback to the previous mechanism in order to avoid the legacy related issues
                    object? rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is not null)
                    {
                        failedMessage = new FailedMessage(rejectedEvent, false);
                    }
                }

                if (failedMessage?.Message is not null && (!failedMessage.ShouldRetry || !hasNextRetry))
                {
                    var failedMessageName = failedMessage.Message.GetType().Name.Underscore();
                    var failedMessageId = Guid.NewGuid().ToString("N");
                    await _publisher.PublishAsync(failedMessage.Message, failedMessageId, correlationId,
                        messageContext: messageContext);
                    _logger.LogError(ex, ex.Message);
                    if (_loggerEnabled)
                    {
                        _logger.LogWarning("Published a failed messaged: {FailedMessageName} with ID: {FailedMessageId}, " +
                                           "Correlation ID: {CorrelationId}, for the message: {MessageName} with ID: {MessageId}",
                            failedMessageName, failedMessageId, correlationId, messageName, messageId);
                    }

                    if (!deadLetterEnabled || !failedMessage.MoveToDeadLetter)
                    {
                        channel.BasicAckAsync(args.DeliveryTag, false);
                        await Task.Yield();
                        return;
                    }
                }

                if (failedMessage is null || failedMessage.ShouldRetry)
                {
                    var errorMessage = $"Unable to handle a message: '{messageName}' with ID: '{messageId}', " +
                                       $"Correlation ID: '{correlationId}', retry {currentRetry}/{_retries}...";

                    _logger.LogError(errorMessage);

                    if (hasNextRetry)
                    {
                        throw new Exception(errorMessage, ex);
                    }
                }

                _logger.LogError("Handling a message: {MessageName} with ID: {MessageId}, Correlation ID: " +
                                 "{CorrelationId} failed", messageName, messageId, correlationId);

                if (failedMessage is not null && !failedMessage.MoveToDeadLetter)
                {
                    channel.BasicAckAsync(args.DeliveryTag, false);
                    await Task.Yield();
                    return;
                }

                if (deadLetterEnabled)
                {
                    _logger.LogError(
                        "Message: {MessageName} with ID: {MessageId}, Correlation ID: " +
                        "{CorrelationId} will be moved to DLX",
                        messageName,
                        messageId,
                        correlationId);
                }

                channel.BasicNackAsync(args.DeliveryTag, false, _requeueFailedMessages);
                await Task.Yield();
            }
        });
    }

    private void Unsubscribe(IMessageSubscriber messageSubscriber)
    {
        var type = messageSubscriber.Type;
        var conventions = _conventionsProvider.Get(type);
        string channelKey = GetChannelKey(conventions);
        if (!_channels.TryRemove(channelKey, out var channel))
        {
            return;
        }

        channel.Dispose();
        _logger.LogTrace($"Removed channel: {channel.ChannelNumber}, " +
                         $"exchange: '{conventions.Exchange}', queue: '{conventions.Queue}', " +
                         $"routing key: '{conventions.RoutingKey}'.");
    }

    private static string GetChannelKey(IConventions conventions)
        => $"{conventions.Exchange}:{conventions.Queue}:{conventions.RoutingKey}";

    public override void Dispose()
    {
        if (_loggerEnabled && _options.Logger?.LogConnectionStatus is true)
        {
            _consumerConnection.CallbackExceptionAsync -= ConnectionOnCallbackExceptionAsync;
            _consumerConnection.ConnectionShutdownAsync -= ConnectionOnConnectionShutdownAsync;
            _consumerConnection.ConnectionBlockedAsync -= ConnectionOnConnectionBlockedAsync;
            _consumerConnection.ConnectionUnblockedAsync -= ConnectionOnConnectionUnblockedAsync;

            _producerConnection.CallbackExceptionAsync -= ConnectionOnCallbackExceptionAsync;
            _producerConnection.ConnectionShutdownAsync -= ConnectionOnConnectionShutdownAsync;
            _producerConnection.ConnectionBlockedAsync -= ConnectionOnConnectionBlockedAsync;
            _producerConnection.ConnectionUnblockedAsync -= ConnectionOnConnectionUnblockedAsync;
        }

        foreach (var (key, channel) in _channels)
        {
            channel?.Dispose();
            _channels.TryRemove(key, out _);
        }

        try
        {
            _consumerConnection.CloseAsync();
            _producerConnection.CloseAsync();
        }
        catch
        {
            // ignored
        }

        base.Dispose();
    }

    private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
    {
        public object? Map(Exception exception, object message) => null;
    }

    private class EmptyExceptionToFailedMessageMapper : IExceptionToFailedMessageMapper
    {
        public FailedMessage? Map(Exception exception, object message) => null;
    }

    private async Task ConnectionOnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs eventArgs)
    {
        _logger.LogError("RabbitMQ callback exception occurred.");
        if (eventArgs.Exception is not null)
        {
            _logger.LogError(eventArgs.Exception, eventArgs.Exception.Message);
        }

        if (eventArgs.Detail is not null)
        {
            _logger.LogError(JsonSerializer.Serialize(eventArgs.Detail, SerializerOptions));
        }

        await Task.CompletedTask;
    }

    private async Task ConnectionOnConnectionShutdownAsync(object sender, ShutdownEventArgs eventArgs)
    {
        _logger.LogError($"RabbitMQ connection shutdown occurred. Initiator: '{eventArgs.Initiator}', " +
                         $"reply code: '{eventArgs.ReplyCode}', text: '{eventArgs.ReplyText}'.");

        await Task.CompletedTask;
    }

    private async Task ConnectionOnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs eventArgs)
    {
        _logger.LogError($"RabbitMQ connection has been blocked. {eventArgs.Reason}");
        await Task.CompletedTask;
    }

    private async Task ConnectionOnConnectionUnblockedAsync(object sender, AsyncEventArgs eventArgs)
    {
        _logger.LogInformation("RabbitMQ connection has been unblocked.");
        await Task.CompletedTask;
    }
}