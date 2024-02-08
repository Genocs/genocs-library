using System.Threading.Tasks;
using System.Threading;

namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// The query dispatcher interface.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// QueryAsync.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// QueryAsync.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <param name="query">The query object instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<TResult?> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>;
}