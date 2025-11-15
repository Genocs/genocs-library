using Genocs.Common.Types;
using Genocs.Core.CQRS.Events;
using Genocs.Core.Extensions;
using Genocs.HTTP;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Genocs.Identities.Application.Decorators;

[Decorator]
internal sealed class LoggingEventHandlerDecorator<TEvent> : IEventHandler<TEvent>
    where TEvent : class, IEvent
{
    private readonly IEventHandler<TEvent> _handler;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ILogger<IEventHandler<TEvent>> _logger;

    public LoggingEventHandlerDecorator(
                                        IEventHandler<TEvent> handler,
                                        ICorrelationIdFactory correlationIdFactory,
                                        ILogger<IEventHandler<TEvent>> logger)
    {
        _handler = handler;
        _correlationIdFactory = correlationIdFactory;
        _logger = logger;
    }

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        string? correlationId = _correlationIdFactory.Create();
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            string? name = @event.GetType().Name.Underscore();
            _logger.LogInformation($"Handling an event: '{name}'...");
            await _handler.HandleAsync(@event);
        }
    }
}