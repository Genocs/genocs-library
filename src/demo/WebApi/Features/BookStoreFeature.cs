using Genocs.Library.Demo.WebApi.BookStore.Contracts;
using Genocs.Library.Demo.WebApi.BookStore.Data;
using Genocs.Library.Demo.WebApi.BookStore.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Library.Demo.WebApi.Features;

public static class BookStoreFeature
{
    public static IEndpointRouteBuilder MapBookStoreFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder rootGroup = endpoints.MapGroup("/api/bookstore").WithTags("BookStore");

        RouteGroupBuilder authorsGroup = rootGroup.MapGroup("/authors");
        authorsGroup.MapGet(string.Empty, GetAuthorsAsync);
        authorsGroup.MapGet("/{id:guid}", GetAuthorByIdAsync);
        authorsGroup.MapPost(string.Empty, CreateAuthorAsync);

        RouteGroupBuilder booksGroup = rootGroup.MapGroup("/books");
        booksGroup.MapGet(string.Empty, GetBooksAsync);
        booksGroup.MapGet("/{id:guid}", GetBookByIdAsync);
        booksGroup.MapPost(string.Empty, CreateBookAsync);
        booksGroup.MapPut("/{id:guid}", UpdateBookAsync);
        booksGroup.MapDelete("/{id:guid}", DeleteBookAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAuthorsAsync(BookStoreDbContext dbContext, CancellationToken cancellationToken)
    {
        List<AuthorResponse> authors = await dbContext.Authors
            .AsNoTracking()
            .OrderBy(author => author.LastName)
            .ThenBy(author => author.FirstName)
            .Select(author => new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography))
            .ToListAsync(cancellationToken);

        return Results.Ok(authors);
    }

    private static async Task<IResult> GetAuthorByIdAsync(BookStoreDbContext dbContext, Guid id, CancellationToken cancellationToken)
    {
        AuthorResponse? author = await dbContext.Authors
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AuthorResponse(item.Id, item.FirstName, item.LastName, item.Biography))
            .SingleOrDefaultAsync(cancellationToken);

        return author is null ? Results.NotFound() : Results.Ok(author);
    }

    private static async Task<IResult> CreateAuthorAsync(BookStoreDbContext dbContext, CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return Results.BadRequest(new { message = "FirstName and LastName are required." });
        }

        Author author = new()
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Biography = string.IsNullOrWhiteSpace(request.Biography) ? null : request.Biography.Trim(),
        };

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/bookstore/authors/{author.Id}", ToAuthorResponse(author));
    }

    private static async Task<IResult> GetBooksAsync(BookStoreDbContext dbContext, CancellationToken cancellationToken)
    {
        List<Book> books = await dbContext.Books
            .AsNoTracking()
            .Include(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.Author)
            .OrderByDescending(book => book.PublishedOnUtc)
            .ThenBy(book => book.Title)
            .ToListAsync(cancellationToken);

        List<BookResponse> response = books
            .Select(ToBookResponse)
            .ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetBookByIdAsync(BookStoreDbContext dbContext, Guid id, CancellationToken cancellationToken)
    {
        Book? book = await LoadBookWithAuthorsAsync(dbContext, id, cancellationToken);

        return book is null ? Results.NotFound() : Results.Ok(ToBookResponse(book));
    }

    private static async Task<IResult> CreateBookAsync(BookStoreDbContext dbContext, CreateBookRequest request, CancellationToken cancellationToken)
    {
        string? requestError = ValidateBookInput(request.Title, request.Isbn, request.Price, request.AuthorIds);
        if (requestError is not null)
        {
            return Results.BadRequest(new { message = requestError });
        }

        Guid[] authorIds = GetDistinctAuthorIds(request.AuthorIds);
        IResult? authorValidationError = await ValidateAuthorsExistAsync(dbContext, authorIds, cancellationToken);
        if (authorValidationError is not null)
        {
            return authorValidationError;
        }

        string normalizedIsbn = request.Isbn.Trim();

        bool isbnExists = await dbContext.Books
            .AsNoTracking()
            .AnyAsync(book => book.Isbn == normalizedIsbn, cancellationToken);
        if (isbnExists)
        {
            return Results.Conflict(new { message = "A book with the same ISBN already exists." });
        }

        Book book = new()
        {
            Title = request.Title.Trim(),
            Isbn = normalizedIsbn,
            Price = request.Price,
            PublishedOnUtc = NormalizeUtc(request.PublishedOnUtc),
        };

        foreach (Guid authorId in authorIds)
        {
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId, BookId = book.Id });
        }

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        Book? createdBook = await LoadBookWithAuthorsAsync(dbContext, book.Id, cancellationToken);
        return Results.Created($"/api/bookstore/books/{book.Id}", ToBookResponse(createdBook!));
    }

    private static async Task<IResult> UpdateBookAsync(BookStoreDbContext dbContext, Guid id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        string? requestError = ValidateBookInput(request.Title, request.Isbn, request.Price, request.AuthorIds);
        if (requestError is not null)
        {
            return Results.BadRequest(new { message = requestError });
        }

        Book? book = await dbContext.Books
            .Include(item => item.BookAuthors)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (book is null)
        {
            return Results.NotFound();
        }

        Guid[] authorIds = GetDistinctAuthorIds(request.AuthorIds);
        IResult? authorValidationError = await ValidateAuthorsExistAsync(dbContext, authorIds, cancellationToken);
        if (authorValidationError is not null)
        {
            return authorValidationError;
        }

        string normalizedIsbn = request.Isbn.Trim();

        bool isbnExists = await dbContext.Books
            .AsNoTracking()
            .AnyAsync(item => item.Isbn == normalizedIsbn && item.Id != id, cancellationToken);
        if (isbnExists)
        {
            return Results.Conflict(new { message = "A book with the same ISBN already exists." });
        }

        book.Title = request.Title.Trim();
        book.Isbn = normalizedIsbn;
        book.Price = request.Price;
        book.PublishedOnUtc = NormalizeUtc(request.PublishedOnUtc);

        book.BookAuthors.Clear();
        foreach (Guid authorId in authorIds)
        {
            book.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        Book? updatedBook = await LoadBookWithAuthorsAsync(dbContext, book.Id, cancellationToken);
        return Results.Ok(ToBookResponse(updatedBook!));
    }

    private static async Task<IResult> DeleteBookAsync(BookStoreDbContext dbContext, Guid id, CancellationToken cancellationToken)
    {
        Book? book = await dbContext.Books
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (book is null)
        {
            return Results.NotFound();
        }

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static Guid[] GetDistinctAuthorIds(IReadOnlyCollection<Guid> authorIds)
    {
        return authorIds
            .Where(authorId => authorId != Guid.Empty)
            .Distinct()
            .ToArray();
    }

    private static async Task<IResult?> ValidateAuthorsExistAsync(BookStoreDbContext dbContext, Guid[] authorIds, CancellationToken cancellationToken)
    {
        List<Guid> existingAuthorIds = await dbContext.Authors
            .AsNoTracking()
            .Where(author => authorIds.Contains(author.Id))
            .Select(author => author.Id)
            .ToListAsync(cancellationToken);

        if (existingAuthorIds.Count == authorIds.Length)
        {
            return null;
        }

        Guid[] missingAuthorIds = authorIds.Except(existingAuthorIds).ToArray();
        return Results.BadRequest(new
        {
            message = "One or more authors were not found.",
            missingAuthorIds,
        });
    }

    private static string? ValidateBookInput(string title, string isbn, decimal price, IReadOnlyCollection<Guid> authorIds)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            return "ISBN is required.";
        }

        if (price < 0)
        {
            return "Price cannot be negative.";
        }

        if (authorIds.Count == 0)
        {
            return "At least one authorId is required.";
        }

        Guid[] distinctAuthorIds = GetDistinctAuthorIds(authorIds);
        if (distinctAuthorIds.Length == 0)
        {
            return "At least one valid authorId is required.";
        }

        return null;
    }

    private static DateTime NormalizeUtc(DateTime date)
    {
        if (date.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return date.ToUniversalTime();
    }

    private static async Task<Book?> LoadBookWithAuthorsAsync(BookStoreDbContext dbContext, Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Include(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.Author)
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    private static AuthorResponse ToAuthorResponse(Author author)
    {
        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography);
    }

    private static BookResponse ToBookResponse(Book book)
    {
        List<AuthorResponse> authors = book.BookAuthors
            .Where(bookAuthor => bookAuthor.Author is not null)
            .Select(bookAuthor => bookAuthor.Author)
            .OrderBy(author => author!.LastName)
            .ThenBy(author => author!.FirstName)
            .Select(author => ToAuthorResponse(author!))
            .ToList();

        return new BookResponse(book.Id, book.Title, book.Isbn, book.Price, book.PublishedOnUtc, authors);
    }
}
