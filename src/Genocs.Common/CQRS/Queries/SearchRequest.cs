namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// The search request class. The search request is used to encapsulate the search query and the maximum number of items to return.
/// </summary>
public class SearchRequest : ISearchRequest
{
    /// <summary>
    /// The search query used for full-text search.
    /// </summary>
    public string q { get; set; } = string.Empty;

    /// <summary>
    /// The maximum number of items to return.
    /// </summary>
    public int MaxItems { get; set; } = 10;
}
