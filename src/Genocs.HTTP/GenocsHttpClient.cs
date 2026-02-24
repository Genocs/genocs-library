using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Genocs.Http.Configurations;
using Polly;

namespace Genocs.Http;

/// <summary>
/// The Genocs Http Client.
/// </summary>
public class GenocsHttpClient : IHttpClient
{
    private readonly HttpClient _client;
    private readonly HttpClientOptions _settings;
    private readonly IHttpClientSerializer _serializer;

    public GenocsHttpClient(
                            HttpClient client,
                            HttpClientOptions settings,
                            IHttpClientSerializer serializer,
                            ICorrelationContextFactory correlationContextFactory,
                            ICorrelationIdFactory correlationIdFactory)
    {
        _client = client;
        _settings = settings;
        _serializer = serializer;
        if (!string.IsNullOrWhiteSpace(_settings.CorrelationContextHeader))
        {
            string correlationContext = correlationContextFactory.Create();
            _client.DefaultRequestHeaders.TryAddWithoutValidation(
                                                                    _settings.CorrelationContextHeader,
                                                                    correlationContext);
        }

        if (!string.IsNullOrWhiteSpace(_settings.CorrelationIdHeader))
        {
            string? correlationId = correlationIdFactory.Create();
            _client.DefaultRequestHeaders.TryAddWithoutValidation(
                                                                    _settings.CorrelationIdHeader,
                                                                    correlationId);
        }
    }

    public virtual Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Get, cancellationToken: cancellationToken);

    public virtual Task<T?> GetAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Get, serializer: serializer, cancellationToken: cancellationToken);
    public Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Get, serializer: serializer, cancellationToken: cancellationToken);

    public virtual Task<HttpResponseMessage> PostAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Post, GetJsonPayload(data, serializer), cancellationToken: cancellationToken);

    public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Post, content, cancellationToken: cancellationToken);

    public virtual Task<T?> PostAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<T?> PostAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Post, content, serializer, cancellationToken);

    public Task<HttpResult<T>> PostResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Post, content, serializer, cancellationToken);

    public virtual Task<HttpResponseMessage> PutAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Put, GetJsonPayload(data, serializer), cancellationToken: cancellationToken);

    public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Put, content, cancellationToken: cancellationToken);

    public virtual Task<T?> PutAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<T?> PutAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Put, content, serializer, cancellationToken);

    public Task<HttpResult<T>> PutResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Put, content, serializer, cancellationToken);

    public Task<HttpResponseMessage> PatchAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Patch, GetJsonPayload(data, serializer), cancellationToken: cancellationToken);

    public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Patch, content, cancellationToken: cancellationToken);

    public Task<T?> PatchAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<T?> PatchAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Patch, content, serializer, cancellationToken);

    public Task<HttpResult<T>> PatchResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer), serializer, cancellationToken);

    public Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Patch, content, serializer, cancellationToken);

    public virtual Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken cancellationToken = default)
        => SendAsync(uri, Method.Delete, cancellationToken: cancellationToken);

    public Task<T?> DeleteAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(uri, Method.Delete, serializer: serializer, cancellationToken: cancellationToken);

    public Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => SendResultAsync<T>(uri, Method.Delete, serializer: serializer, cancellationToken: cancellationToken);

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(() => _client.SendAsync(request, cancellationToken));

    public Task<T?> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                // Send the Http request
                var response = await _client.SendAsync(request, cancellationToken);

                // Check if the response indicates a successful status code
                if (!response.IsSuccessStatusCode)
                {
                    // If not successful, throw an exception so the retry will come.
                    throw new HttpRequestException($"The Http request failed with status code {response.StatusCode}.");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                return await DeserializeJsonFromStream<T>(stream, serializer, cancellationToken);
            });

    public Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                var response = await _client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new HttpResult<T>(default!, response);
                }

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var result = await DeserializeJsonFromStream<T>(stream, serializer, cancellationToken);

                return new HttpResult<T>(result, response);
            });

    public void SetHeaders(IDictionary<string, string> headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var (key, value) in headers.Where(h => !string.IsNullOrEmpty(h.Key)))
        {
            _client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
    }

    public void SetHeaders(Action<HttpRequestHeaders> headers)
        => headers?.Invoke(_client.DefaultRequestHeaders);

    protected virtual async Task<T?> SendAsync<T>(string uri, Method method, HttpContent? content = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(uri, method, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default!;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await DeserializeJsonFromStream<T>(stream, serializer);
    }

    protected virtual async Task<HttpResult<T>> SendResultAsync<T>(string uri, Method method, HttpContent? content = null, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(uri, method, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new HttpResult<T>(default!, response);
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await DeserializeJsonFromStream<T>(stream, serializer, cancellationToken);

        return new HttpResult<T>(result, response);
    }

    protected virtual Task<HttpResponseMessage> SendAsync(string uri, Method method, HttpContent? content = null, CancellationToken cancellationToken = default)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                string requestUri = uri.StartsWith("http") ? uri : $"http://{uri}";

                var result = await GetResponseAsync(requestUri, method, content, cancellationToken) ?? throw new HttpRequestException("The Http request failed.");

                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception($"The Http request failed with status code {result.StatusCode}.");
                }

                return result;
            });

    protected virtual Task<HttpResponseMessage> GetResponseAsync(string uri, Method method, HttpContent? content = null, CancellationToken cancellationToken = default)
        => method switch
        {
            Method.Get => _client.GetAsync(uri, cancellationToken),
            Method.Post => _client.PostAsync(uri, content, cancellationToken),
            Method.Put => _client.PutAsync(uri, content, cancellationToken),
            Method.Patch => _client.PatchAsync(uri, content, cancellationToken),
            Method.Delete => _client.DeleteAsync(uri, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported Http method: {method}")
        };

    protected StringContent? GetJsonPayload(object? data, IHttpClientSerializer? serializer = null)
    {
        if (data is null)
        {
            return null;
        }

        serializer ??= _serializer;
        var content = new StringContent(serializer.Serialize(data), Encoding.UTF8, MediaTypeNames.Application.Json);
        if (_settings.RemoveCharsetFromContentType && content.Headers.ContentType is not null)
        {
            content.Headers.ContentType.CharSet = null;
        }

        return content;
    }

    protected async Task<T?> DeserializeJsonFromStream<T>(Stream stream, IHttpClientSerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        if (stream is null || stream.CanRead is false)
        {
            return default!;
        }

        serializer ??= _serializer;
        return await serializer.DeserializeAsync<T>(stream, cancellationToken);
    }

    protected enum Method
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }
}