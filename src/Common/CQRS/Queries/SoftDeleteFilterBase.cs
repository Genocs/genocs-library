namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Reusable request model base for soft-delete visibility filtering.
/// </summary>
public abstract class SoftDeleteFilterBase : ISoftDeleteFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether soft-deleted records should be included.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="false"/> to keep deleted records hidden in standard flows.
    /// </remarks>
    public bool IncludeDeleted { get; set; }
}