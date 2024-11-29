using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Genocs.MessageBrokers.RabbitMQ.Clients;

internal sealed class RabbitMQClient : IRabbitMQClient
{
    private const string EmptyContext = "{}";
    private readonly IConnection _connection;
    private readonly IContextProvider _contextProvider;
    private readonly IRabbitMQSerializer _serializer;
    private readonly ILogger<RabbitMQClient> _logger;
    private readonly bool _contextEnabled;
    private readonly bool _loggerEnabled;
    private readonly string _spanContextHeader;
    private readonly bool _persistMessages;
    private readonly ConcurrentDictionary<int, IChannel> _channels = new();
    private readonly int _maxChannels;
    private int _channelsCount;

    public RabbitMQClient(
                            ProducerConnection connection,
                            IContextProvider contextProvider,
                            IRabbitMQSerializer serializer,
                            RabbitMQOptions options,
                            ILogger<RabbitMQClient> logger)
    {
        _connection = connection.Connection;
        _contextProvider = contextProvider;
        _serializer = serializer;
        _logger = logger;
        _contextEnabled = options.Context?.Enabled == true;
        _loggerEnabled = options.Logger?.Enabled ?? false;
        _spanContextHeader = options.GetSpanContextHeader();
        _persistMessages = options?.MessagesPersisted ?? false;
        _maxChannels = options.MaxProducerChannels <= 0 ? 1000 : options.MaxProducerChannels;
    }

    public async Task SendAsync(
                                    object message,
                                    IConventions conventions,
                                    string? messageId = null,
                                    string? correlationId = null,
                                    string? spanContext = null,
                                    object? messageContext = null,
                                    IDictionary<string, object>? headers = null)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        if (!_channels.TryGetValue(threadId, out var channel))
        {
            if (_channelsCount >= _maxChannels)
            {
                throw new InvalidOperationException($"Cannot create RabbitMQ producer channel for thread: {threadId} " +
                                                    $"(reached the limit of {_maxChannels} channels). " +
                                                    "Modify `MaxProducerChannels` setting to allow more channels.");

            }

            channel = await _connection.CreateChannelAsync();
            _channels.TryAdd(threadId, channel);
            _channelsCount++;
            if (_loggerEnabled)
            {
                _logger.LogTrace($"Created a channel for thread: {threadId}, total channels: {_channelsCount}/{_maxChannels}");
            }

        }
        else
        {
            if (_loggerEnabled)
            {
                _logger.LogTrace($"Reused a channel for thread: {threadId}, total channels: {_channelsCount}/{_maxChannels}");
            }
        }

        var body = _serializer.Serialize(message);

        BasicProperties properties = new BasicProperties();

        // var properties = channel.BasicProperties();
        properties.Persistent = _persistMessages;

        properties.MessageId = string.IsNullOrWhiteSpace(messageId)
            ? Guid.NewGuid().ToString("N")
            : messageId;

        properties.CorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? Guid.NewGuid().ToString("N")
            : correlationId;

        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        properties.Headers = new Dictionary<string, object?>();

        if (_contextEnabled)
        {
            IncludeMessageContext(messageContext, properties);
        }

        if (!string.IsNullOrWhiteSpace(spanContext))
        {
            properties.Headers.Add(_spanContextHeader, spanContext);
        }

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                if (string.IsNullOrWhiteSpace(key) || value is null)
                {
                    continue;
                }

                properties.Headers.TryAdd(key, value);
            }
        }

        if (_loggerEnabled)
        {
            _logger.LogTrace($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                             $"to exchange: '{conventions.Exchange}' " +
                             $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
        }

        await channel.BasicPublishAsync(conventions.Exchange, conventions.RoutingKey, true, properties, body.ToArray());
    }

    private void IncludeMessageContext(object? context, IBasicProperties properties)
    {
        if (context is null)
            return;

        if (properties.Headers is null)
            return;

        if (context is not null)
        {
            properties.Headers.Add(_contextProvider.HeaderName, _serializer.Serialize(context).ToArray());
            return;
        }

        properties.Headers.Add(_contextProvider.HeaderName, EmptyContext);
    }
}