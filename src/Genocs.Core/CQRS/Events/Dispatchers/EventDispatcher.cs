using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Events.Dispatchers;

/// <summary>
/// The event dispatcher is responsible for dispatching events to their respective handlers.
/// </summary>
internal sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// standard constructor.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public EventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    /// <summary>
    /// The event dispatcher is responsible for dispatching events to their respective handlers.
    /// </summary>
    /// <typeparam name="T">The type of the event to publish.</typeparam>
    /// <param name="event">The event object instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">The event cannot be null.</exception>
    /// <returns>Async task.</returns>
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent
    {
        if (@event is null)
        {
            throw new InvalidOperationException("Event cannot be null.");
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();
        var tasks = handlers.Select(x => x.HandleAsync(@event, cancellationToken));
        await Task.WhenAll(tasks);
    }
}