namespace Genocs.Core.Base
{
    public class PagedQueryWithFilter : PagedQueryBase
    {
        public string FilterBy { get; set; } = string.Empty;
    }
}