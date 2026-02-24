using Genocs.Common.Cqrs.Commands;
using Genocs.Common.Cqrs.Events;
using Genocs.Common.Cqrs.Queries;

namespace Genocs.WebApi.Cqrs;

public interface IDispatcher
{
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;

    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;

    Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}