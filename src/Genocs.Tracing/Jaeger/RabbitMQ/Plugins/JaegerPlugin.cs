using Genocs.Core.Extensions;
using Genocs.MessageBrokers.RabbitMQ;
using Jaeger;
using OpenTracing;
using OpenTracing.Tag;
using RabbitMQ.Client.Events;
using System.Text;

namespace Genocs.Tracing.Jaeger.RabbitMQ.Plugins;

internal sealed class JaegerPlugin : RabbitMQPlugin
{
    private readonly ITracer _tracer;
    private readonly string _spanContextHeader;

    public JaegerPlugin(ITracer tracer, RabbitMQOptions options)
    {
        _tracer = tracer;
        _spanContextHeader = options.GetSpanContextHeader();
    }

    public override async Task HandleAsync(
                                            object message,
                                            object correlationContext,
                                            BasicDeliverEventArgs args)
    {
        string messageName = message.GetType().Name.Underscore();
        string messageId = args.BasicProperties.MessageId;
        string spanContext = string.Empty;

        if (args.BasicProperties.Headers is { } &&
            args.BasicProperties.Headers.TryGetValue(_spanContextHeader, out object? spanContextHeader) &&
            spanContextHeader is byte[] spanContextBytes)
        {
            spanContext = Encoding.UTF8.GetString(spanContextBytes);
        }

        using var scope = BuildScope(messageName, spanContext);
        var span = scope.Span;
        span.Log($"Started processing a message: '{messageName}' [id: '{messageId}'].");
        try
        {
            await Next(message, correlationContext, args);
        }
        catch (Exception ex)
        {
            span.SetTag(Tags.Error, true);
            span.Log(ex.Message);
        }

        span.Log($"Finished processing a message: '{messageName}' [id: '{messageId}'].");
    }

    private IScope BuildScope(string messageName, string serializedSpanContext)
    {
        var spanBuilder = _tracer
            .BuildSpan($"processing-{messageName}")
            .WithTag("message-type", messageName);

        if (string.IsNullOrEmpty(serializedSpanContext))
        {
            return spanBuilder.StartActive(true);
        }

        var spanContext = SpanContext.ContextFromString(serializedSpanContext);

        return spanBuilder
            .AddReference(References.FollowsFrom, spanContext)
            .StartActive(true);
    }
}