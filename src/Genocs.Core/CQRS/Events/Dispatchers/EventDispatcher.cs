namespace Genocs.Core.CQRS.Events.Dispatchers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Genocs.Core.CQRS.Events;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
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
}