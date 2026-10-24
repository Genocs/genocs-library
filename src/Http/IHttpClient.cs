using System.Net.Http.Headers;

namespace Genocs.Http;

/// <summary>
/// The Genocs Http client.
/// This interface defines a set of methods for sending Http requests and receiving Http responses
/// from a resource identified by a URI. It provides methods for common Http operations such as
/// GET, POST, PUT, PATCH, and DELETE, as well as a generic SendAsync method for sending custom Http requests.
/// The interface also includes methods for setting Http headers and supports
/// serialization and deserialization of request and response bodies using an optional serializer.
/// <para>
/// Methods that return <see cref="HttpResult{T}"/> (names ending in <c>ResultAsync</c> with a string URI)
/// return a result wrapper for both success and non-success HTTP status codes; they do not throw solely because
/// the status code indicates failure. Methods that return <c>T?</c> or <see cref="HttpResponseMessage"/> for
/// string-based URIs use an exception-oriented path that throws when the response status is not successful.
/// </para>
/// <para>
/// String URI parameters must be valid absolute or relative URIs as accepted by <see cref="System.Uri"/>.
/// Absolute URIs (for example <c>https://api.contoso.com/v1/items</c>) are sent unchanged. Relative paths
/// (for example <c>items/5</c>) combine with <see cref="System.Net.Http.HttpClient.BaseAddress"/> when it is set.
/// The client does not prepend schemes or rewrite host-like strings; configure the base address (or pass absolute URIs) explicitly.
/// </para>
/// </summary>
public interface IHttpClient
{
    Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the resource and returns both the deserialized body (when successful) and the raw response,
    /// without throwing on non-success status codes.
    /// </summary>
    Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts data and returns a result wrapper that preserves non-success responses.
    /// </summary>
    Task<HttpResult<T>> PostResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts content and returns a result wrapper that preserves non-success responses.
    /// </summary>
    Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PutAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>Puts data and returns a result wrapper that preserves non-success responses.</summary>
    Task<HttpResult<T>> PutResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>Puts content and returns a result wrapper that preserves non-success responses.</summary>
    Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>Patches data and returns a result wrapper that preserves non-success responses.</summary>
    Task<HttpResult<T>> PatchResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>Patches content and returns a result wrapper that preserves non-success responses.</summary>
    Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken cancellationToken = default);
    Task<T?> DeleteAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes the resource and returns a result wrapper that preserves non-success responses.</summary>
    Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// It sends the Http request and returns the Http response message. This method allows for sending custom Http requests
    /// and provides a cancellation token to cancel the operation if needed.
    /// The provided <see cref="HttpRequestMessage"/> instance is sent once per call and is not replayed by internal retries.
    /// </summary>
    /// <param name="request">The Http request message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The Http response message.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a custom request and deserializes the body on success; throws when the response status is not successful.
    /// The provided <see cref="HttpRequestMessage"/> instance is sent once per call and is not replayed by internal retries.
    /// </summary>
    /// <typeparam name="T">The type to be send.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The return object.</returns>
    Task<T?> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a custom request and returns a result wrapper that preserves non-success HTTP status codes (no throw solely for status).
    /// The provided <see cref="HttpRequestMessage"/> instance is sent once per call and is not replayed by internal retries.
    /// </summary>
    /// <typeparam name="T">The type to be send.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The HttpResult object.</returns>
    Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the headers for the Http client.
    /// </summary>
    /// <param name="headers">The headers dictionary.</param>
    void SetHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Set the headers for the Http client.
    /// </summary>
    /// <param name="headers">The headers action delegate.</param>
    void SetHeaders(Action<HttpRequestHeaders> headers);
}