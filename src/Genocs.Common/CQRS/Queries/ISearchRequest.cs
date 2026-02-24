namespace Genocs.Common.Cqrs.Queries;

/// <summary>
/// The search request interface. The search request is used to encapsulate the search query and the maximum number of items to return.
/// </summary>
public interface ISearchRequest
{
    /// <summary>
    /// The search query used for full-text search.
    /// </summary>
    string q { get; set; }

    /// <summary>
    /// The maximum number of items to return.
    /// </summary>
    int MaxItems { get; set; }
}