using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;

namespace Genocs.Core.CQRS.Commons;

/// <summary>
/// Generic dispatcher interface.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Generic command sender.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;

    /// <summary>
    /// Generic event publisher 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;

    /// <summary>
    /// Generic query fetcher.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
