namespace Genocs.Core.CQRS.Queries
{
    /// <summary>
    /// Query for pagination 
    /// </summary>
    public interface IPagedQuery : IQuery
    {
        /// <summary>
        /// Page to query zero indexed
        /// </summary>
        int Page { get; }

        /// <summary>
        /// Number of results. Aka page size
        /// </summary>
        int Results { get; }

        /// <summary>
        /// The field used to order by 
        /// </summary>
        string? OrderBy { get; }

        /// <summary>
        /// Type of order. It could be ASC or DESC
        /// </summary>
        string? SortOrder { get; }
    }
}