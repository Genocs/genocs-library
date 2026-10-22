using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;

namespace Genocs.WebApi.CQRS;

public class InMemoryDispatcher(ICommandDispatcher commandDispatcher, IEventDispatcher eventDispatcher, IQueryDispatcher queryDispatcher) : IDispatcher
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

    public Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand
        => _commandDispatcher.SendAsync(command, cancellationToken);

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent
        => _eventDispatcher.PublishAsync(@event, cancellationToken);

    public Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => _queryDispatcher.QueryAsync(query, cancellationToken);
}