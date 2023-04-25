namespace Genocs.Core.CQRS.Events;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The CQRS event dispatcher interface used to publish an integration event
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// It allows to Publish an integration event Async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
}