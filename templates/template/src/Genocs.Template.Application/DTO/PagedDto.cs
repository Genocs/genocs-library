namespace Genocs.Template.Application.DTO;


/// <summary>
/// It will be removed with Core implementation
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedDto<T>
{
    public IEnumerable<T> Items { get; set; }
    public bool Empty => Items is null || !Items.Any();
    public int CurrentPage { get; set; }
    public int ResultsPerPage { get; set; }
    public int TotalPages { get; set; }
    public long TotalResults { get; set; }
}