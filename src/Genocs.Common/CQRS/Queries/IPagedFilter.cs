namespace Genocs.Common.Cqrs.Queries;

/// <summary>
/// The paged filter interface.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TQuery">The type of the query.</typeparam>
public interface IPagedFilter<TResult, in TQuery>
    where TQuery : IQuery
{
    /// <summary>
    /// Filters the given values based on the specified query.
    /// </summary>
    /// <param name="values">The values to filter.</param>
    /// <param name="query">The query to apply.</param>
    /// <returns>A paged result containing the filtered values.</returns>
    PagedResult<TResult> Filter(IEnumerable<TResult> values, TQuery query);
}
