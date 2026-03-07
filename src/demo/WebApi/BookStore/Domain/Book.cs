namespace Genocs.Library.Demo.WebApi.BookStore.Domain;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime PublishedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<BookAuthor> BookAuthors { get; } = new List<BookAuthor>();
}
