using System.Diagnostics;

namespace Genocs.Saga;

/// <summary>
/// W3C trace context metadata keys for saga correlation with distributed tracing (e.g. Jaeger).
/// Use these when building saga context from messages to propagate trace context across services.
/// </summary>
public static class SagaTraceContext
{
    /// <summary>
    /// W3C traceparent header format: 00-{trace-id}-{span-id}-{trace-flags}.
    /// </summary>
    public const string TraceParent = "traceparent";

    /// <summary>
    /// W3C tracestate header for vendor-specific trace state.
    /// </summary>
    public const string TraceState = "tracestate";

    /// <summary>
    /// Adds the current Activity trace context to the saga context builder for correlation.
    /// Call this when processing messages to link saga execution to the incoming request trace.
    /// </summary>
    public static ISagaContextBuilder WithCurrentTraceContext(this ISagaContextBuilder builder)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return builder;
        }

        string? traceParent = activity.Id;
        if (string.IsNullOrEmpty(traceParent))
        {
            return builder;
        }

        builder.WithMetadata(TraceParent, traceParent);

        string? traceState = activity.TraceStateString;
        if (!string.IsNullOrEmpty(traceState))
        {
            builder.WithMetadata(TraceState, traceState);
        }

        return builder;
    }

    /// <summary>
    /// Adds trace context from W3C headers to the saga context builder.
    /// Use when building context from message headers (e.g. RabbitMQ, Kafka, HTTP).
    /// </summary>
    public static ISagaContextBuilder WithTraceContext(
        this ISagaContextBuilder builder,
        string traceParent,
        string? traceState = null)
    {
        if (string.IsNullOrWhiteSpace(traceParent))
        {
            return builder;
        }

        builder.WithMetadata(TraceParent, traceParent.Trim());

        if (!string.IsNullOrWhiteSpace(traceState))
        {
            builder.WithMetadata(TraceState, traceState.Trim());
        }

        return builder;
    }
}
