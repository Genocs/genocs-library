namespace Genocs.Library.Demo.WebApi.BookStore.Domain;

public class Author
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<BookAuthor> BookAuthors { get; } = new List<BookAuthor>();
}
