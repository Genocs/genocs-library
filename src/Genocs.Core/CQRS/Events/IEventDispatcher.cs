using Genocs.Common.CQRS.Events;

namespace Genocs.Core.CQRS.Events;

/// <summary>
/// The CQRS event dispatcher interface used to publish an integration event.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// It allows to Publish an integration event Async.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;
}