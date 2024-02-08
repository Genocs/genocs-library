namespace Genocs.Core.CQRS.Events;

/// <summary>
/// Generic interface for CQRS Event handler.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Standard Event handler
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Legacy Event handler interface definition
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventHandlerLegacy<T> where T : IEvent
{
    /// <summary>
    /// Legacy event handler place holder.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    Task HandleEvent(T @event);
}