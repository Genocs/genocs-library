namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Query for pagination.
/// </summary>
public interface IPagedQuery : IQuery
{
    /// <summary>
    /// Page to query, zero-indexed.
    /// </summary>
    /// <remarks>
    /// Minimum valid value is 0.
    /// </remarks>
    int Page { get; }

    /// <summary>
    /// Number of results, also known as page size.
    /// </summary>
    /// <remarks>
    /// Minimum valid value is 1.
    /// Recommended maximum is 100.
    /// Validation should be enforced by handlers or infrastructure validators.
    /// </remarks>
    int Results { get; }

    /// <summary>
    /// The field used to order by.
    /// </summary>
    string? OrderBy { get; }

    /// <summary>
    /// Type of order. It could be ASC or DESC.
    /// </summary>
    string? SortOrder { get; }
}