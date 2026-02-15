using System.Net.Http.Headers;

namespace Genocs.HTTP;

/// <summary>
/// The Genocs HTTP client.
/// This interface defines a set of methods for sending HTTP requests and receiving HTTP responses
/// from a resource identified by a URI. It provides methods for common HTTP operations such as
/// GET, POST, PUT, PATCH, and DELETE, as well as a generic SendAsync method for sending custom HTTP requests.
/// The interface also includes methods for setting HTTP headers and supports
/// serialization and deserialization of request and response bodies using an optional serializer.
/// </summary>
public interface IHttpClient
{
    Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PostResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PutResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PatchResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken cancellationToken = default);
    Task<T?> DeleteAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// It sends the HTTP request and returns the HTTP response message. This method allows for sending custom HTTP requests
    /// and provides a cancellation token to cancel the operation if needed.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send the request and return the result.
    /// </summary>
    /// <typeparam name="T">The type to be send.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The return object.</returns>
    Task<T?> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send the request and return the result.
    /// </summary>
    /// <typeparam name="T">The type to be send.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The HttpResult object.</returns>
    Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the headers for the HTTP client.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    void SetHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Set the headers for the HTTP client.
    /// </summary>
    /// <param name="headers">The headers action delegate.</param>
    void SetHeaders(Action<HttpRequestHeaders> headers);
}