namespace Genocs.Library.Demo.WebApi.BookStore.Domain;

public class BookAuthor
{
    public Guid BookId { get; set; }

    public Book Book { get; set; } = null!;

    public Guid AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}
