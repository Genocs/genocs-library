using System.Net.Http.Json;
using System.Text.Json;
using Genocs.Library.Demo.WebApi.BookStore.Contracts;

namespace Genocs.Library.Demo.ContractTests;

/// <summary>
/// API client that simulates the BookStore API consumer for Pact contract testing.
/// </summary>
public sealed class BookStoreApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public BookStoreApiClient(Uri baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress, "api/bookstore/"),
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IReadOnlyList<AuthorResponse>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("authors", cancellationToken);
        response.EnsureSuccessStatusCode();
        List<AuthorResponse>? authors = await response.Content.ReadFromJsonAsync<List<AuthorResponse>>(JsonOptions, cancellationToken);
        return authors ?? [];
    }

    public async Task<AuthorResponse?> GetAuthorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"authors/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorResponse>(JsonOptions, cancellationToken);
    }

    public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("authors", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        AuthorResponse? author = await response.Content.ReadFromJsonAsync<AuthorResponse>(JsonOptions, cancellationToken);
        return author ?? throw new InvalidOperationException("Failed to deserialize created author.");
    }

    public async Task<IReadOnlyList<BookResponse>> GetBooksAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("books", cancellationToken);
        response.EnsureSuccessStatusCode();
        List<BookResponse>? books = await response.Content.ReadFromJsonAsync<List<BookResponse>>(JsonOptions, cancellationToken);
        return books ?? [];
    }

    public async Task<BookResponse?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"books/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookResponse>(JsonOptions, cancellationToken);
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("books", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        BookResponse? book = await response.Content.ReadFromJsonAsync<BookResponse>(JsonOptions, cancellationToken);
        return book ?? throw new InvalidOperationException("Failed to deserialize created book.");
    }

    public async Task<BookResponse> UpdateBookAsync(Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"books/{id}", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        BookResponse? book = await response.Content.ReadFromJsonAsync<BookResponse>(JsonOptions, cancellationToken);
        return book ?? throw new InvalidOperationException("Failed to deserialize updated book.");
    }

    public async Task DeleteBookAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"books/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
