namespace Genocs.Common.CQRS.Events;

/// <summary>
/// Marker contract for integration events that must participate in a durable outbox flow.
/// </summary>
public interface ITransactionalEvent : IIntegrationEvent;

/// <summary>
/// Non-generic outbox message envelope for integration event publication intent.
/// </summary>
public interface IOutboxMessage : IOutboxMessage<IIntegrationEvent>;

/// <summary>
/// Generic outbox message envelope for integration event publication intent.
/// </summary>
/// <typeparam name="TIntegrationEvent">The integration event type.</typeparam>
public interface IOutboxMessage<out TIntegrationEvent>
    where TIntegrationEvent : class, IIntegrationEvent
{
    /// <summary>
    /// Gets the durable message identifier.
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Gets the correlation identifier used to link related operations.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the outbox intent was captured.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the integration event payload to publish.
    /// </summary>
    TIntegrationEvent IntegrationEvent { get; }
}

/// <summary>
/// Contract for persisting integration-event publication intent to a durable outbox.
/// </summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// Enqueues an integration event for durable outbox publication.
    /// </summary>
    /// <typeparam name="TIntegrationEvent">The integration event type.</typeparam>
    /// <param name="integrationEvent">The integration event payload.</param>
    /// <param name="messageId">Optional explicit durable message identifier.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="occurredAt">Optional UTC event timestamp; implementations may default this value when omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous enqueue operation.</returns>
    Task EnqueueAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        string? messageId = null,
        string? correlationId = null,
        DateTime? occurredAt = null,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : class, IIntegrationEvent;
}
