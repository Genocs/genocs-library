using System.Diagnostics;

namespace Genocs.Saga;

/// <summary>
/// Telemetry support for saga execution, enabling correlation with distributed tracing (e.g. Jaeger).
/// </summary>
internal static class SagaTelemetry
{
    public static readonly ActivitySource ActivitySource = new("Genocs.Saga", "1.0.0");

    /// <summary>
    /// Attempts to extract an ActivityContext from saga context metadata (e.g. from message headers).
    /// </summary>
    public static bool TryGetParentContext(ISagaContext? context, out ActivityContext parentContext)
    {
        parentContext = default;

        if (context is null || !context.TryGetMetadata(SagaTraceContext.TraceParent, out var traceParentMeta) || traceParentMeta is null)
        {
            return false;
        }

        string? traceParent = traceParentMeta.Value as string;
        if (string.IsNullOrWhiteSpace(traceParent))
        {
            return false;
        }

        string? traceState = null;
        if (context.TryGetMetadata(SagaTraceContext.TraceState, out var traceStateMeta) && traceStateMeta is not null)
        {
            traceState = traceStateMeta.Value as string;
        }

        return ActivityContext.TryParse(traceParent, traceState, out parentContext);
    }

    /// <summary>
    /// Gets the parent context for a new span: from saga context metadata, or from Activity.Current.
    /// </summary>
    public static ActivityContext GetParentContext(ISagaContext? context)
    {
        if (TryGetParentContext(context, out var parentContext))
        {
            return parentContext;
        }

        return Activity.Current?.Context ?? default;
    }

    /// <summary>
    /// Starts an activity for saga processing with standard tags.
    /// </summary>
    public static Activity? StartProcessActivity(ISagaContext? context, string messageType)
    {
        var parentContext = GetParentContext(context);
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("saga.message.type", messageType)
        };

        return ActivitySource.StartActivity(
            "Saga.Process",
            ActivityKind.Internal,
            parentContext,
            tags);
    }

    /// <summary>
    /// Starts an activity for a single saga execution with standard tags.
    /// </summary>
    public static Activity? StartExecuteActivity(ISagaContext? context, SagaId sagaId, string sagaType, string messageType)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("saga.id", sagaId.ToString()),
            new("saga.type", sagaType),
            new("saga.message.type", messageType)
        };

        return ActivitySource.StartActivity("Saga.Execute", ActivityKind.Internal, parentContext: default, tags);
    }

    /// <summary>
    /// Starts an activity for saga action handling with standard tags.
    /// </summary>
    public static Activity? StartHandleActivity(SagaId sagaId, string sagaType, string messageType)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("saga.id", sagaId.ToString()),
            new("saga.type", sagaType),
            new("saga.message.type", messageType)
        };

        return ActivitySource.StartActivity("Saga.Handle", ActivityKind.Internal, parentContext: default, tags);
    }

    /// <summary>
    /// Starts an activity for saga compensation with standard tags.
    /// </summary>
    public static Activity? StartCompensateActivity(SagaId sagaId, string sagaType)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("saga.id", sagaId.ToString()),
            new("saga.type", sagaType)
        };

        return ActivitySource.StartActivity("Saga.Compensate", ActivityKind.Internal, parentContext: default, tags);
    }

    /// <summary>
    /// Records saga state and error on the current activity.
    /// </summary>
    public static void SetSagaOutcome(SagaProcessState state, SagaContextError? error = null)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        activity.SetTag("saga.state", state.ToString());

        if (error is not null)
        {
            activity.SetStatus(ActivityStatusCode.Error, error.Exception.Message);
            activity.SetTag("error.type", error.Exception.GetType().FullName);
            activity.SetTag("error.message", error.Exception.Message);
        }
    }
}
