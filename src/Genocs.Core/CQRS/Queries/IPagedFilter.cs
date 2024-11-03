namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// The paged filter interface.
/// </summary>
/// <typeparam name="TResult"></typeparam>
/// <typeparam name="TQuery"></typeparam>
public interface IPagedFilter<TResult, in TQuery>
    where TQuery : IQuery
{
    /// <summary>
    /// Filter.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    PagedResult<TResult> Filter(IEnumerable<TResult> values, TQuery query);
}