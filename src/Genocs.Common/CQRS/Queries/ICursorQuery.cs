namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Query contract for cursor-based pagination.
/// </summary>
public interface ICursorQuery : IQuery
{
    /// <summary>
    /// Opaque cursor token representing a continuation position.
    /// </summary>
    string? Cursor { get; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// Optional field used to order the cursor query.
    /// </summary>
    string? OrderBy { get; }

    /// <summary>
    /// Optional sort order. Common values are ASC or DESC.
    /// </summary>
    string? SortOrder { get; }
}
