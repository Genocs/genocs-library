using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;

namespace Genocs.Common.CQRS.Commons;

/// <summary>
/// Generic dispatcher interface.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Generic command sender.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;

    /// <summary>
    /// Generic event publisher.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IEvent;

    /// <summary>
    /// Generic query fetcher.
    /// </summary>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the query result.</returns>
    Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
