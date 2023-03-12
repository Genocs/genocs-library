namespace Genocs.Core.CQRS.Events
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventDispatcher
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    }
}