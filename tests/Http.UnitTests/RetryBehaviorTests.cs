using System.Net;
using System.Text.Json;
using Genocs.Http.Configurations;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class RetryBehaviorTests
{
    private sealed class StubCorrelationContextFactory : ICorrelationContextFactory
    {
        public string? Create() => string.Empty;
    }

    private sealed class StubCorrelationIdFactory : ICorrelationIdFactory
    {
        public string? Create() => string.Empty;
    }

    private sealed class SequenceHandler(IEnumerable<object> outcomes) : HttpMessageHandler
    {
        private readonly Queue<object> _outcomes = new(outcomes);

        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Attempts++;

            if (_outcomes.Count == 0)
            {
                throw new InvalidOperationException("No configured outcome for this send attempt.");
            }

            var outcome = _outcomes.Dequeue();
            return outcome switch
            {
                Exception exception => Task.FromException<HttpResponseMessage>(exception),
                HttpResponseMessage response => Task.FromResult(response),
                _ => throw new InvalidOperationException($"Unsupported outcome type: {outcome.GetType().Name}")
            };
        }
    }

    private sealed class TrackingJsonContent(string json) : StringContent(json, System.Text.Encoding.UTF8, "application/json")
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }

    private sealed class SingleResponseHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Attempts++;
            return Task.FromResult(response);
        }
    }

    private sealed class CancellationAwareHandler : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Attempts++;
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TokenCapturingSerializer : IHttpClientSerializer
    {
        public CancellationToken LastToken { get; private set; }

        public string Serialize<T>(T value) => "{}";

        public ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            object? result = typeof(T) == typeof(Dictionary<string, string>)
                ? new Dictionary<string, string>()
                : default(T);

            return ValueTask.FromResult((T?)result);
        }
    }

    private static GenocsHttpClient CreateClient(
        HttpMessageHandler handler,
        int retries,
        bool retryUnsafeHttpMethods = false,
        IHttpClientSerializer? serializer = null)
    {
        var options = new HttpClientOptions
        {
            Retries = retries,
            RetryUnsafeHttpMethods = retryUnsafeHttpMethods
        };

        return new GenocsHttpClient(
            new HttpClient(handler),
            options,
            serializer ?? new SystemTextJsonHttpClientSerializer(),
            new StubCorrelationContextFactory(),
            new StubCorrelationIdFactory());
    }

    [Fact]
    public async Task GetAsync_retries_transient_transport_failures()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
        ]);

        var sut = CreateClient(handler, retries: 2);

        var response = await sut.GetAsync("https://example.test/retry-get");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, handler.Attempts);
    }

    [Fact]
    public async Task PostAsync_does_not_retry_transport_failures_by_default()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
        ]);

        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: false);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.PostAsync("https://example.test/retry-post"));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task PostAsync_retries_transport_failures_when_enabled()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
        ]);

        var sut = CreateClient(handler, retries: 1, retryUnsafeHttpMethods: true);

        var response = await sut.PostAsync("https://example.test/retry-post");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_does_not_retry_non_success_status_codes()
    {
        using var handler = new SequenceHandler([
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        ]);

        var sut = CreateClient(handler, retries: 3);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetAsync("https://example.test/status"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task SendAsync_of_T_does_not_retry_deserialization_failures()
    {
        using var handler = new SequenceHandler([
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{invalid-json}", System.Text.Encoding.UTF8, "application/json")
            }
        ]);

        var sut = CreateClient(handler, retries: 3);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/json");

        await Assert.ThrowsAsync<JsonException>(() => sut.SendAsync<Dictionary<string, string>>(request));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_of_T_does_not_retry_deserialization_failures_for_string_uri()
    {
        using var handler = new SequenceHandler([
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{invalid-json}", System.Text.Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            }
        ]);

        var sut = CreateClient(handler, retries: 3);

        await Assert.ThrowsAsync<JsonException>(() => sut.GetAsync<Dictionary<string, string>>("https://example.test/json"));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task SendAsync_with_HttpRequestMessage_does_not_retry_transport_failures()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
        ]);

        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: true);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/custom-request");

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.SendAsync(request));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task SendAsync_of_T_with_HttpRequestMessage_does_not_retry_transport_failures()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            }
        ]);

        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: true);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/custom-request");

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.SendAsync<Dictionary<string, string>>(request));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task SendResultAsync_with_content_request_does_not_retry_transport_failures()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Temporary network error"),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            }
        ]);

        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: true);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/custom-request")
        {
            Content = new StringContent("{\"name\":\"test\"}", System.Text.Encoding.UTF8, "application/json")
        };

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.SendResultAsync<Dictionary<string, string>>(request));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_does_not_retry_when_operation_is_cancelled()
    {
        using var handler = new CancellationAwareHandler();
        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: true);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.GetAsync("https://example.test/cancelled", cts.Token));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_does_not_retry_cancellation_wrapped_in_http_request_exception()
    {
        using var handler = new SequenceHandler([
            new HttpRequestException("Request canceled.", new TaskCanceledException()),
            new HttpResponseMessage(HttpStatusCode.OK)
        ]);

        var sut = CreateClient(handler, retries: 3, retryUnsafeHttpMethods: true);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetAsync("https://example.test/cancelled"));

        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_of_T_propagates_cancellation_token_to_serializer()
    {
        using var handler = new CancellationAwareHandler();
        var serializer = new TokenCapturingSerializer();
        var sut = CreateClient(handler, retries: 0, serializer: serializer);
        using var cts = new CancellationTokenSource();

        _ = await sut.GetAsync<Dictionary<string, string>>("https://example.test/payload", cancellationToken: cts.Token);

        Assert.Equal(cts.Token, serializer.LastToken);
    }

    [Fact]
    public async Task SendAsync_of_T_disposes_response_content_after_successful_deserialization()
    {
        var trackingContent = new TrackingJsonContent("{}");
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = trackingContent
        };
        using var handler = new SingleResponseHandler(response);
        var sut = CreateClient(handler, retries: 0);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/dispose-success");

        _ = await sut.SendAsync<Dictionary<string, string>>(request);

        Assert.True(trackingContent.IsDisposed);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task SendAsync_of_T_disposes_response_content_before_throwing_on_non_success_status()
    {
        var trackingContent = new TrackingJsonContent("{}");
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = trackingContent
        };
        using var handler = new SingleResponseHandler(response);
        var sut = CreateClient(handler, retries: 0);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/dispose-failure");

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.SendAsync<Dictionary<string, string>>(request));

        Assert.True(trackingContent.IsDisposed);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task GetAsync_of_T_disposes_response_content_when_non_success_throws()
    {
        var trackingContent = new TrackingJsonContent("{}");
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = trackingContent
        };
        using var handler = new SingleResponseHandler(response);
        var sut = CreateClient(handler, retries: 0);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetAsync<Dictionary<string, string>>("https://example.test/dispose-default"));

        Assert.True(trackingContent.IsDisposed);
        Assert.Equal(1, handler.Attempts);
    }
}
