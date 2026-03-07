namespace Genocs.Library.Demo.WebApi.BookStore.Contracts;

public sealed record CreateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography);

public sealed record AuthorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Biography);

public sealed record CreateBookRequest(
    string Title,
    string Isbn,
    decimal Price,
    DateTime PublishedOnUtc,
    IReadOnlyCollection<Guid> AuthorIds);

public sealed record UpdateBookRequest(
    string Title,
    string Isbn,
    decimal Price,
    DateTime PublishedOnUtc,
    IReadOnlyCollection<Guid> AuthorIds);

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Isbn,
    decimal Price,
    DateTime PublishedOnUtc,
    IReadOnlyCollection<AuthorResponse> Authors);
