using Genocs.Core.CQRS.Events;
using Genocs.Core.Extensions;
using Genocs.HTTP;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Genocs.Identities.Application.Services;

internal class MessageBroker : IMessageBroker
{
    private const string DefaultSpanContextHeader = "span_context";
    private readonly IBusPublisher _busPublisher;
    private readonly IMessageOutbox _outbox;
    private readonly ICorrelationContextAccessor _contextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ILogger<IMessageBroker> _logger;
    private readonly string _spanContextHeader;

    public MessageBroker(
                            IBusPublisher busPublisher,
                            IMessageOutbox outbox,
                            ICorrelationContextAccessor contextAccessor,
                            IHttpContextAccessor httpContextAccessor,
                            IMessagePropertiesAccessor messagePropertiesAccessor,
                            ICorrelationIdFactory correlationIdFactory,
                            RabbitMQOptions options,
                            ILogger<IMessageBroker> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _messagePropertiesAccessor = messagePropertiesAccessor ?? throw new ArgumentNullException(nameof(messagePropertiesAccessor));
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _spanContextHeader = string.IsNullOrWhiteSpace(options.SpanContextHeader)
            ? DefaultSpanContextHeader
            : options.SpanContextHeader;
    }

    public Task PublishAsync(params IEvent[] events) => PublishAsync(events?.AsEnumerable());

    private async Task PublishAsync(IEnumerable<IEvent>? events)
    {
        if (events is null)
        {
            return;
        }

        var messageProperties = _messagePropertiesAccessor.MessageProperties;
        string? originatedMessageId = messageProperties?.MessageId;
        string? correlationId = _correlationIdFactory.Create();
        string? spanContext = messageProperties?.GetSpanContext(_spanContextHeader);
        object correlationContext = _contextAccessor.CorrelationContext ?? _httpContextAccessor.GetCorrelationContext();

        var headers = new Dictionary<string, object>();

        foreach (var @event in events)
        {
            if (@event is null)
            {
                continue;
            }

            string messageId = Guid.NewGuid().ToString("N");
            _logger.LogTrace($"Publishing integration event: {@event.GetType().Name.Underscore()} [ID: '{messageId}'].");
            if (_outbox.Enabled)
            {
                await _outbox.SendAsync(@event, originatedMessageId, messageId, correlationId, spanContext, correlationContext, headers);
                continue;
            }

            await _busPublisher.PublishAsync(@event, messageId, correlationId, spanContext, correlationContext, headers);
        }
    }
}