﻿namespace Genocs.Core.CQRS.Commons
{
    using Genocs.Core.CQRS.Commands;
    using Genocs.Core.CQRS.Events;
    using Genocs.Core.CQRS.Queries;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// The class name will be renamed!!!
    /// </summary>
    internal sealed class InMemoryDispatcher : IDispatcher
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public InMemoryDispatcher(ICommandDispatcher commandDispatcher, IEventDispatcher eventDispatcher,
            IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _eventDispatcher = eventDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
            => _commandDispatcher.SendAsync(command, cancellationToken);

        public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
            => _eventDispatcher.PublishAsync(@event, cancellationToken);

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
            => _queryDispatcher.QueryAsync(query, cancellationToken);
    }
}
