namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// The paged query result.
/// </summary>
public abstract class PagedQueryBase : IPagedQuery
{
    /// <summary>
    /// The zero-based page index.
    /// </summary>
    /// <remarks>
    /// Default value is 0 (the first page).
    /// Minimum valid value is 0.
    /// </remarks>
    public int Page { get; set; } = 0;

    /// <summary>
    /// Number of results, also known as page size.
    /// </summary>
    /// <remarks>
    /// Default value is 10.
    /// Minimum valid value is 1.
    /// Recommended maximum is 100.
    /// </remarks>
    public int Results { get; set; } = 10;

    /// <summary>
    /// The field used to order by.
    /// </summary>
    public string OrderBy { get; set; }

    /// <summary>
    /// Type of order. It could be ASC or DESC.
    /// </summary>
    public string SortOrder { get; set; }
}