namespace Genocs.Core.CQRS.Queries
{
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// The query dispatcher interface
    /// </summary>
    public interface IQueryDispatcher
    {
        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : class, IQuery<TResult>;
    }
}