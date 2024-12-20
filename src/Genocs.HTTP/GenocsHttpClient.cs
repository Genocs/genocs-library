using Genocs.HTTP.Configurations;
using Polly;
using System.Net.Http.Headers;
using System.Text;

namespace Genocs.HTTP;

/// <summary>
/// The Genocs Http Client.
/// </summary>
public class GenocsHttpClient : IHttpClient
{
    private const string JsonContentType = "application/json";
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

    public virtual Task<HttpResponseMessage> GetAsync(string uri)
        => SendAsync(uri, Method.Get);

    public virtual Task<T?> GetAsync<T>(string uri, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Get, serializer: serializer);

    public Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Get, serializer: serializer);

    public virtual Task<HttpResponseMessage> PostAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync(uri, Method.Post, GetJsonPayload(data, serializer));

    public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
        => SendAsync(uri, Method.Post, content);

    public virtual Task<T?> PostAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer));

    public Task<T?> PostAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Post, content, serializer);

    public Task<HttpResult<T>> PostResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer), serializer);

    public Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Post, content, serializer);

    public virtual Task<HttpResponseMessage> PutAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync(uri, Method.Put, GetJsonPayload(data, serializer));

    public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content)
        => SendAsync(uri, Method.Put, content);

    public virtual Task<T?> PutAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer);

    public Task<T?> PutAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Put, content, serializer);

    public Task<HttpResult<T>> PutResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer);

    public Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Put, content, serializer);

    public Task<HttpResponseMessage> PatchAsync(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync(uri, Method.Patch, GetJsonPayload(data, serializer));

    public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content)
        => SendAsync(uri, Method.Patch, content);

    public Task<T?> PatchAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer));

    public Task<T?> PatchAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Patch, content, serializer);

    public Task<HttpResult<T>> PatchResultAsync<T>(string uri, object? data = null, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer));

    public Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Patch, content, serializer);

    public virtual Task<HttpResponseMessage> DeleteAsync(string uri)
        => SendAsync(uri, Method.Delete);

    public Task<T?> DeleteAsync<T>(string uri, IHttpClientSerializer? serializer = null)
        => SendAsync<T>(uri, Method.Delete, serializer: serializer);

    public Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer? serializer = null)
        => SendResultAsync<T>(uri, Method.Delete, serializer: serializer);

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(() => _client.SendAsync(request));

    public Task<T?> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                // Send the HTTP request
                var response = await _client.SendAsync(request);

                // Check if the response indicates a successful status code
                if (!response.IsSuccessStatusCode)
                {
                    // If not successful, throw an exception so the retry will come.
                    throw new HttpRequestException($"The HTTP request failed with status code {response.StatusCode}.");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                return await DeserializeJsonFromStream<T>(stream, serializer);
            });

    public Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer? serializer = null)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                var response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return new HttpResult<T>(default!, response);
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var result = await DeserializeJsonFromStream<T>(stream, serializer);

                return new HttpResult<T>(result, response);
            });

    public void SetHeaders(IDictionary<string, string> headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var (key, value) in headers)
        {
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            _client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
    }

    public void SetHeaders(Action<HttpRequestHeaders> headers)
        => headers?.Invoke(_client.DefaultRequestHeaders);

    protected virtual async Task<T?> SendAsync<T>(string uri, Method method, HttpContent? content = null, IHttpClientSerializer? serializer = null)
    {
        var response = await SendAsync(uri, method, content);
        if (!response.IsSuccessStatusCode)
        {
            return default!;
        }

        var stream = await response.Content.ReadAsStreamAsync();

        return await DeserializeJsonFromStream<T>(stream, serializer);
    }

    protected virtual async Task<HttpResult<T>> SendResultAsync<T>(string uri, Method method, HttpContent? content = null, IHttpClientSerializer? serializer = null)
    {
        var response = await SendAsync(uri, method, content);
        if (!response.IsSuccessStatusCode)
        {
            return new HttpResult<T>(default!, response);
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var result = await DeserializeJsonFromStream<T>(stream, serializer);

        return new HttpResult<T>(result, response);
    }

    protected virtual Task<HttpResponseMessage> SendAsync(string uri, Method method, HttpContent? content = null)
        => Policy.Handle<Exception>()
            .WaitAndRetryAsync(_settings.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
            .ExecuteAsync(async () =>
            {
                string requestUri = uri.StartsWith("http") ? uri : $"http://{uri}";

                var result = await GetResponseAsync(requestUri, method, content) ?? throw new HttpRequestException("The HTTP request failed.");

                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception($"The HTTP request failed with status code {result.StatusCode}.");
                }

                return result;
            });

    protected virtual Task<HttpResponseMessage> GetResponseAsync(string uri, Method method, HttpContent? content = null)
        => method switch
        {
            Method.Get => _client.GetAsync(uri),
            Method.Post => _client.PostAsync(uri, content),
            Method.Put => _client.PutAsync(uri, content),
            Method.Patch => _client.PatchAsync(uri, content),
            Method.Delete => _client.DeleteAsync(uri),
            _ => throw new InvalidOperationException($"Unsupported HTTP method: {method}")
        };

    protected StringContent? GetJsonPayload(object? data, IHttpClientSerializer? serializer = null)
    {
        if (data is null)
        {
            return null;
        }

        serializer ??= _serializer;
        var content = new StringContent(serializer.Serialize(data), Encoding.UTF8, JsonContentType);
        if (_settings.RemoveCharsetFromContentType && content.Headers.ContentType is not null)
        {
            content.Headers.ContentType.CharSet = null;
        }

        return content;
    }

    protected async Task<T?> DeserializeJsonFromStream<T>(Stream stream, IHttpClientSerializer? serializer = null)
    {
        if (stream is null || stream.CanRead is false)
        {
            return default!;
        }

        serializer ??= _serializer;
        return await serializer.DeserializeAsync<T>(stream);
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