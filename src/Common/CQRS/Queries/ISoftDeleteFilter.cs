namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Query contract that controls soft-deleted record visibility.
/// </summary>
public interface ISoftDeleteFilter
{
    /// <summary>
    /// Gets a value indicating whether soft-deleted records should be included in query results.
    /// </summary>
    /// <remarks>
    /// Default behavior should exclude deleted records unless explicitly enabled.
    /// </remarks>
    bool IncludeDeleted { get; }
}