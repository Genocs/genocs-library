using System.Text.Json.Serialization;

namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// The paged result.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedResult<T> : PagedResultBase
{
    /// <summary>
    /// Returned items
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    protected PagedResult()
    {
        Items = Enumerable.Empty<T>();
    }

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="items"></param>
    /// <param name="currentPage"></param>
    /// <param name="resultsPerPage"></param>
    /// <param name="totalPages"></param>
    /// <param name="totalResults"></param>
    [JsonConstructor]
    protected PagedResult(IEnumerable<T> items,
                          int currentPage,
                          int resultsPerPage,
                          int totalPages,
                          long totalResults) :
            base(currentPage, resultsPerPage, totalPages, totalResults)
    {
        Items = items;
    }

    /// <summary>
    /// Create helper
    /// </summary>
    /// <param name="items"></param>
    /// <param name="currentPage"></param>
    /// <param name="resultsPerPage"></param>
    /// <param name="totalPages"></param>
    /// <param name="totalResults"></param>
    /// <returns></returns>
    public static PagedResult<T> Create(IEnumerable<T> items,
        int currentPage, int resultsPerPage,
        int totalPages, long totalResults)
        => new PagedResult<T>(items, currentPage, resultsPerPage, totalPages, totalResults);

    /// <summary>
    /// From helper
    /// </summary>
    /// <param name="result"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static PagedResult<T> From(PagedResultBase result, IEnumerable<T> items)
        => new PagedResult<T>(items, result.CurrentPage, result.ResultsPerPage,
            result.TotalPages, result.TotalResults);

    /// <summary>
    /// Static helper to get Empty result
    /// </summary>
    public static PagedResult<T> Empty => new PagedResult<T>();
}