namespace Genocs.Core.CQRS.Queries
{
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// The query handler interface
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : class, IQuery<TResult>
    {
        /// <summary>
        /// HandleAsync
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult?> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}