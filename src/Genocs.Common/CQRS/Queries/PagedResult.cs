using System.Text.Json.Serialization;

namespace Genocs.Common.Cqrs.Queries;

/// <summary>
/// The paged result.
/// </summary>
/// <typeparam name="T">The page result type.</typeparam>
public class PagedResult<T> : PagedResultBase
{
    /// <summary>
    /// Returned items.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected PagedResult()
    {
        Items = Enumerable.Empty<T>();
    }

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentPage">Zero based current page.</param>
    /// <param name="resultsPerPage">Number of results within the page.</param>
    /// <param name="totalPages">Total number of pages.</param>
    /// <param name="totalResults">Total number of results.</param>
    [JsonConstructor]
    protected PagedResult(
                          IEnumerable<T> items,
                          int currentPage,
                          int resultsPerPage,
                          int totalPages,
                          long totalResults)
        : base(currentPage, resultsPerPage, totalPages, totalResults)
    {
        Items = items;
    }

    /// <summary>
    /// Create helper.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentPage">Zero based current page.</param>
    /// <param name="resultsPerPage">Number of results within the page.</param>
    /// <param name="totalPages">Total number of pages.</param>
    /// <param name="totalResults">Total number of results.</param>
    /// <returns>The paged results.</returns>
    public static PagedResult<T> Create(
                                        IEnumerable<T> items,
                                        int currentPage,
                                        int resultsPerPage,
                                        int totalPages,
                                        long totalResults)
        => new(items, currentPage, resultsPerPage, totalPages, totalResults);

    /// <summary>
    /// From helper.
    /// </summary>
    /// <param name="result">The base paged result.</param>
    /// <param name="items">The list of items.</param>
    /// <returns>The paged result with the specified items.</returns>
    public static PagedResult<T> From(
                                      PagedResultBase result,
                                      IEnumerable<T> items)
        => new(
                items,
                result.CurrentPage,
                result.ResultsPerPage,
                result.TotalPages,
                result.TotalResults);

    /// <summary>
    /// Static helper to get an empty result.
    /// </summary>
    public static PagedResult<T> Empty
        => new();
}
