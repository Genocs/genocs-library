namespace Genocs.Identities.Application.DTO;

/// <summary>
/// It will be removed with Core implementation.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public class PagedDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public bool Empty => Items is null || !Items.Any();
    public int CurrentPage { get; set; }
    public int ResultsPerPage { get; set; }
    public int TotalPages { get; set; }
    public long TotalResults { get; set; }
}