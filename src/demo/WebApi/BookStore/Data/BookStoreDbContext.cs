using Genocs.Library.Demo.WebApi.BookStore.Domain;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Library.Demo.WebApi.BookStore.Data;

public sealed class BookStoreDbContext : DbContext
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => this.Set<Author>();

    public DbSet<Book> Books => this.Set<Book>();

    public DbSet<BookAuthor> BookAuthors => this.Set<BookAuthor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Authors");
            entity.HasKey(author => author.Id);

            entity.Property(author => author.FirstName)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(author => author.LastName)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(author => author.Biography)
                .HasMaxLength(2000);

            entity.Property(author => author.CreatedAtUtc)
                .HasPrecision(0);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.HasKey(book => book.Id);

            entity.Property(book => book.Title)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(book => book.Isbn)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(book => book.Isbn)
                .IsUnique();

            entity.Property(book => book.Price)
                .HasPrecision(18, 2);

            entity.Property(book => book.PublishedOnUtc)
                .HasPrecision(0);

            entity.Property(book => book.CreatedAtUtc)
                .HasPrecision(0);
        });

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.ToTable("BookAuthors");
            entity.HasKey(bookAuthor => new { bookAuthor.BookId, bookAuthor.AuthorId });

            entity.HasOne(bookAuthor => bookAuthor.Book)
                .WithMany(book => book.BookAuthors)
                .HasForeignKey(bookAuthor => bookAuthor.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bookAuthor => bookAuthor.Author)
                .WithMany(author => author.BookAuthors)
                .HasForeignKey(bookAuthor => bookAuthor.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(bookAuthor => bookAuthor.AuthorId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
