using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Commons;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;

namespace Genocs.Core.CQRS.Commons;

/// <summary>
/// The InMemoryDispatcher class is an implementation of the IDispatcher interface
/// that uses in-memory dispatching for commands, events, and queries.
/// It serves as a central point for sending commands, publishing events,
/// and executing queries within the application.
/// This implementation is suitable for scenarios where you want to keep the dispatching logic simple
/// and do not require external messaging systems or infrastructure.
/// </summary>
internal sealed class InMemoryDispatcher : IDispatcher
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public InMemoryDispatcher(ICommandDispatcher commandDispatcher, IEventDispatcher eventDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
    }

    public Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand
        => _commandDispatcher.SendAsync(command, cancellationToken);

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent
        => _eventDispatcher.PublishAsync(@event, cancellationToken);

    public Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => _queryDispatcher.QueryAsync(query, cancellationToken);
}
