namespace Genocs.Core.CQRS.Queries
{
    public interface ISearchRequest
    {
        string q { get; set; }

        int MaxItems { get; set; }
    }

    public class SearchRequest : ISearchRequest
    {
        public string q { get; set; } = string.Empty;

        public int MaxItems { get; set; } = 10;
    }
}
