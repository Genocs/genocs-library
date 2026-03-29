namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// The search request interface. The search request is used to encapsulate the search query and the maximum number of items to return.
/// </summary>
public interface ISearchRequest
{
    /// <summary>
    /// The search term used for full-text search.
    /// </summary>
    string SearchTerm
    {
        get => q;
        set => q = value;
    }

    /// <summary>
    /// Compatibility alias for legacy query binding.
    /// Use <see cref="SearchTerm"/> in new code.
    /// </summary>
    string q { get; set; }

    /// <summary>
    /// The maximum number of items to return.
    /// </summary>
    int MaxItems { get; set; }
}