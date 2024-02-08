namespace Genocs.Core.CQRS.Queries;

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
    /// Default constructor.
    /// </summary>
    protected PagedResultBase()
    {
    }

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="currentPage"></param>
    /// <param name="resultsPerPage"></param>
    /// <param name="totalPages"></param>
    /// <param name="totalResults"></param>
    protected PagedResultBase(int currentPage, int resultsPerPage, int totalPages, long totalResults)
    {
        CurrentPage = currentPage > totalPages ? totalPages : currentPage;
        ResultsPerPage = resultsPerPage;
        TotalPages = totalPages;
        TotalResults = totalResults;
    }
}