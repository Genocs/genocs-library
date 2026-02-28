namespace Genocs.Common.CQRS.Events;

/// <summary>
/// Generic interface for CQRS Event handler.
/// </summary>
/// <typeparam name="TEvent">The type of the event.</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : class, IEvent
{
    /// <summary>
    /// Standard Event handler.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Legacy Event handler interface definition.
/// </summary>
/// <typeparam name="T">The type of the event.</typeparam>
public interface IEventHandlerLegacy<T>
    where T : IEvent
{
    /// <summary>
    /// Legacy event handler place holder.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleEvent(T @event);
}
