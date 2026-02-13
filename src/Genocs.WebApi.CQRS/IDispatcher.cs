using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;

namespace Genocs.WebApi.CQRS;

public interface IDispatcher
{
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;

    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;

    Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}