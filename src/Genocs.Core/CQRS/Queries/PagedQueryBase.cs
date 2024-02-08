namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// The paged query result.
/// </summary>
public abstract class PagedQueryBase : IPagedQuery
{
    /// <summary>
    /// The zero based page index.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of results. Aka page size.
    /// </summary>
    public int Results { get; set; }

    /// <summary>
    /// The field used to order by.
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Type of order. It could be ASC or DESC.
    /// </summary>
    public string? SortOrder { get; set; }
}