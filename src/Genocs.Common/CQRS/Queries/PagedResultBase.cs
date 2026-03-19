namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// The page result base class.
/// </summary>
public abstract class PagedResultBase
{
    /// <summary>
    /// Returned zero index page.
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    /// Number of returned results. Aka page size.
    /// </summary>
    public int ResultsPerPage { get; }

    /// <summary>
    /// Number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Total number of results.
    /// </summary>
    public long TotalResults { get; }

    /// <summary>
    /// Gets whether there are more pages.
    /// </summary>
    public bool HasNextPage => CurrentPage < (TotalPages - 1);

    /// <summary>
    /// Gets whether there are previous pages.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected PagedResultBase()
    {
    }

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="currentPage">The current page index (zero-based).</param>
    /// <param name="resultsPerPage">The number of results per page.</param>
    /// <param name="totalPages">The total number of pages.</param>
    /// <param name="totalResults">The total number of results.</param>
    protected PagedResultBase(int currentPage, int resultsPerPage, int totalPages, long totalResults)
    {
        CurrentPage = currentPage > totalPages ? totalPages : currentPage;
        ResultsPerPage = resultsPerPage;
        TotalPages = totalPages;
        TotalResults = totalResults;
    }
}