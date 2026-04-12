namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Base implementation for cursor-based paged queries.
/// </summary>
public abstract class CursorQueryBase : ICursorQuery
{
    /// <summary>
    /// Opaque cursor token representing the starting point for the query.
    /// Null means the first window.
    /// </summary>
    public string Cursor { get; set; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    /// <remarks>
    /// Default value is 10.
    /// Minimum valid value is 1.
    /// Recommended maximum is 100.
    /// </remarks>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Optional field used to order the cursor query.
    /// </summary>
    public string OrderBy { get; set; }

    /// <summary>
    /// Optional sort order. Common values are ASC or DESC.
    /// </summary>
    public string SortOrder { get; set; }
}
