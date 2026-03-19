using System.Net;
using System.Text.Json;
using Genocs.Library.Demo.WebApi.BookStore.Contracts;
using PactNet;

namespace Genocs.Library.Demo.ContractTests;

/// <summary>
/// Pact consumer contract tests for the BookStore API.
/// Defines and verifies HTTP interactions between the BookStore API Consumer and the BookStore API provider.
/// </summary>
public class WebApiContractTests
{
    private readonly IPactBuilderV4 _pactBuilder;

    public WebApiContractTests()
    {
        var pact = Pact.V4("BookStore API Consumer", "BookStore API", new PactConfig
        {
            PactDir = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName,
                "pacts"),
        });

        _pactBuilder = pact.WithHttpInteractions();
    }

    [Fact]
    public async Task GetAuthors_WhenAuthorsExist_ReturnsAuthorsList()
    {
        // Arrange
        _pactBuilder
            .UponReceiving("A GET request to retrieve all authors")
                .Given("Authors exist in the bookstore")
                .WithRequest(HttpMethod.Get, "/api/bookstore/authors")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new[]
                {
                    new
                    {
                        id = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                        firstName = "Stephen",
                        lastName = "King",
                        biography = "American author known for horror and suspense."
                    },
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            IReadOnlyList<AuthorResponse> authors = await client.GetAuthorsAsync();

            Assert.NotEmpty(authors);
            Assert.Equal("Stephen", authors[0].FirstName);
            Assert.Equal("King", authors[0].LastName);
        });
    }

    [Fact]
    public async Task GetAuthorById_WhenAuthorExists_ReturnsAuthor()
    {
        // Arrange
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        _pactBuilder
            .UponReceiving("A GET request to retrieve an author by id")
                .Given("An author with the given id exists")
                .WithRequest(HttpMethod.Get, $"/api/bookstore/authors/{authorId}")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = authorId,
                    firstName = "George R. R.",
                    lastName = "Martin",
                    biography = "Author of A Song of Ice and Fire."
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            AuthorResponse? author = await client.GetAuthorByIdAsync(authorId);

            Assert.NotNull(author);
            Assert.Equal(authorId, author.Id);
            Assert.Equal("George R. R.", author.FirstName);
            Assert.Equal("Martin", author.LastName);
        });
    }

    [Fact]
    public async Task GetAuthorById_WhenAuthorDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        _pactBuilder
            .UponReceiving("A GET request to retrieve a non-existent author")
                .Given("No author with the given id exists")
                .WithRequest(HttpMethod.Get, $"/api/bookstore/authors/{authorId}")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            AuthorResponse? author = await client.GetAuthorByIdAsync(authorId);

            Assert.Null(author);
        });
    }

    [Fact]
    public async Task CreateAuthor_WithValidRequest_ReturnsCreatedAuthor()
    {
        // Arrange
        var request = new CreateAuthorRequest("J.K.", "Rowling", "Author of Harry Potter.");
        Guid expectedId = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7");

        _pactBuilder
            .UponReceiving("A POST request to create an author")
                .Given("The request is valid")
                .WithRequest(HttpMethod.Post, "/api/bookstore/authors")
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new
                {
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    biography = request.Biography,
                })
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = expectedId,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    biography = request.Biography,
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            AuthorResponse author = await client.CreateAuthorAsync(request);

            Assert.Equal(expectedId, author.Id);
            Assert.Equal(request.FirstName, author.FirstName);
            Assert.Equal(request.LastName, author.LastName);
        });
    }

    [Fact]
    public async Task GetBooks_WhenBooksExist_ReturnsBooksList()
    {
        // Arrange
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        _pactBuilder
            .UponReceiving("A GET request to retrieve all books")
                .Given("Books exist in the bookstore")
                .WithRequest(HttpMethod.Get, "/api/bookstore/books")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new[]
                {
                    new
                    {
                        id = "5fa85f64-5717-4562-b3fc-2c963f66afa8",
                        title = "The Stand",
                        isbn = "978-0307743688",
                        price = 14.99m,
                        publishedOnUtc = "1978-10-03T00:00:00Z",
                        authors = new[]
                        {
                            new
                            {
                                id = authorId,
                                firstName = "Stephen",
                                lastName = "King",
                                biography = (string?)null
                            },
                        },
                    },
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            IReadOnlyList<BookResponse> books = await client.GetBooksAsync();

            Assert.NotEmpty(books);
            Assert.Equal("The Stand", books[0].Title);
            Assert.Equal("978-0307743688", books[0].Isbn);
            Assert.Equal(14.99m, books[0].Price);
            Assert.Single(books[0].Authors);
            Assert.Equal("Stephen", books[0].Authors.First().FirstName);
        });
    }

    [Fact]
    public async Task GetBookById_WhenBookExists_ReturnsBook()
    {
        // Arrange
        Guid bookId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa8");
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

        _pactBuilder
            .UponReceiving("A GET request to retrieve a book by id")
                .Given("A book with the given id exists")
                .WithRequest(HttpMethod.Get, $"/api/bookstore/books/{bookId}")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = bookId,
                    title = "A Game of Thrones",
                    isbn = "978-0553593716",
                    price = 12.99m,
                    publishedOnUtc = "1996-08-01T00:00:00Z",
                    authors = new[]
                    {
                        new
                        {
                            id = authorId,
                            firstName = "George R. R.",
                            lastName = "Martin",
                            biography = (string?)null
                        },
                    },
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            BookResponse? book = await client.GetBookByIdAsync(bookId);

            Assert.NotNull(book);
            Assert.Equal(bookId, book.Id);
            Assert.Equal("A Game of Thrones", book.Title);
            Assert.Equal("978-0553593716", book.Isbn);
            Assert.Single(book.Authors);
        });
    }

    [Fact]
    public async Task CreateBook_WithValidRequest_ReturnsCreatedBook()
    {
        // Arrange
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        Guid expectedBookId = Guid.Parse("6fa85f64-5717-4562-b3fc-2c963f66afa9");
        var request = new CreateBookRequest(
            "Harry Potter and the Philosopher's Stone",
            "978-0747532699",
            9.99m,
            new DateTime(1997, 6, 26, 0, 0, 0, DateTimeKind.Utc),
            [authorId]);

        JsonElement requestBody = JsonSerializer.SerializeToElement(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        _pactBuilder
            .UponReceiving("A POST request to create a book")
                .Given("An author exists and the request is valid")
                .WithRequest(HttpMethod.Post, "/api/bookstore/books")
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(requestBody)
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = expectedBookId,
                    title = request.Title,
                    isbn = request.Isbn,
                    price = request.Price,
                    publishedOnUtc = request.PublishedOnUtc.ToString("O"),
                    authors = new[]
                    {
                        new
                        {
                            id = authorId,
                            firstName = "J.K.",
                            lastName = "Rowling",
                            biography = (string?)null
                        },
                    },
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            BookResponse book = await client.CreateBookAsync(request);

            Assert.Equal(expectedBookId, book.Id);
            Assert.Equal(request.Title, book.Title);
            Assert.Equal(request.Isbn, book.Isbn);
        });
    }

    [Fact]
    public async Task UpdateBook_WithValidRequest_ReturnsUpdatedBook()
    {
        // Arrange
        Guid bookId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa8");
        Guid authorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var request = new UpdateBookRequest(
            "The Stand - Extended Edition",
            "978-0307743688",
            19.99m,
            new DateTime(1990, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            [authorId]);

        JsonElement requestBody = JsonSerializer.SerializeToElement(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        _pactBuilder
            .UponReceiving("A PUT request to update a book")
                .Given("A book with the given id exists")
                .WithRequest(HttpMethod.Put, $"/api/bookstore/books/{bookId}")
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(requestBody)
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = bookId,
                    title = request.Title,
                    isbn = request.Isbn,
                    price = request.Price,
                    publishedOnUtc = request.PublishedOnUtc.ToString("O"),
                    authors = new[]
                    {
                        new
                        {
                            id = authorId,
                            firstName = "Stephen",
                            lastName = "King",
                            biography = (string?)null
                        },
                    },
                });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            BookResponse book = await client.UpdateBookAsync(bookId, request);

            Assert.Equal(bookId, book.Id);
            Assert.Equal(request.Title, book.Title);
            Assert.Equal(request.Price, book.Price);
        });
    }

    [Fact]
    public async Task DeleteBook_WhenBookExists_ReturnsNoContent()
    {
        // Arrange
        Guid bookId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa8");

        _pactBuilder
            .UponReceiving("A DELETE request to delete a book")
                .Given("A book with the given id exists")
                .WithRequest(HttpMethod.Delete, $"/api/bookstore/books/{bookId}")
            .WillRespond()
                .WithStatus(HttpStatusCode.NoContent);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            var client = new BookStoreApiClient(ctx.MockServerUri);
            await client.DeleteBookAsync(bookId);
        });
    }
}
