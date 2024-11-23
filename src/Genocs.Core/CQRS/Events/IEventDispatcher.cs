namespace Genocs.Core.CQRS.Events;

/// <summary>
/// The CQRS event dispatcher interface used to publish an integration event.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// It allows to Publish an integration event Async.
    /// </summary>
    /// <typeparam name="T">The Type of the event.</typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;
}