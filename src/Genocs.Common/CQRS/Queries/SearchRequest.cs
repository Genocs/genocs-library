namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// The search request class. The search request is used to encapsulate the search query and the maximum number of items to return.
/// </summary>
public class SearchRequest : ISearchRequest
{
    /// <summary>
    /// The search term used for full-text search.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Compatibility alias for legacy query binding.
    /// Use <see cref="SearchTerm"/> in new code.
    /// </summary>
    public string q
    {
        get => SearchTerm;
        set => SearchTerm = value;
    }

    /// <summary>
    /// The maximum number of items to return.
    /// </summary>
    public int MaxItems { get; set; } = 10;
}
