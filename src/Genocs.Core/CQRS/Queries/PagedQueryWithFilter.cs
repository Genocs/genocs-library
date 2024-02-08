namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// Paged query extension with Filter.
/// </summary>
public class PagedQueryWithFilter : PagedQueryBase
{
    /// <summary>
    /// The filter.
    /// </summary>
    public string FilterBy { get; set; } = string.Empty;
}