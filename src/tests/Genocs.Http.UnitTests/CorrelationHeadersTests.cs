using System.Net;
using Genocs.Http.Configurations;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class CorrelationHeadersTests
{
    private sealed class StaticCorrelationContextFactory(string? value) : ICorrelationContextFactory
    {
        public string? Create() => value;
    }

    private sealed class StaticCorrelationIdFactory(string? value) : ICorrelationIdFactory
    {
        public string? Create() => value;
    }

    private sealed class CapturingHeadersHandler : HttpMessageHandler
    {
        public HttpRequestHeadersSnapshot? LastHeaders { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastHeaders = new HttpRequestHeadersSnapshot(
                request.Headers.TryGetValues("x-correlation-context", out var correlationContextValues)
                    ? correlationContextValues.ToArray()
                    : [],
                request.Headers.TryGetValues("x-correlation-id", out var correlationIdValues)
                    ? correlationIdValues.ToArray()
                    : []);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed record HttpRequestHeadersSnapshot(string[] CorrelationContextValues, string[] CorrelationIdValues);

    [Fact]
    public async Task Null_or_whitespace_correlation_values_are_not_added_as_headers()
    {
        using var handler = new CapturingHeadersHandler();
        var httpClient = new HttpClient(handler);
        var options = new HttpClientOptions
        {
            Retries = 0,
            CorrelationContextHeader = "x-correlation-context",
            CorrelationIdHeader = "x-correlation-id"
        };
        var sut = new GenocsHttpClient(
            httpClient,
            options,
            new SystemTextJsonHttpClientSerializer(),
            new StaticCorrelationContextFactory(null),
            new StaticCorrelationIdFactory("   "));

        await sut.GetAsync("https://example.test/resource");

        Assert.NotNull(handler.LastHeaders);
        Assert.Empty(handler.LastHeaders!.CorrelationContextValues);
        Assert.Empty(handler.LastHeaders.CorrelationIdValues);
    }

    [Fact]
    public async Task Non_empty_correlation_values_are_added_as_headers()
    {
        using var handler = new CapturingHeadersHandler();
        var httpClient = new HttpClient(handler);
        var options = new HttpClientOptions
        {
            Retries = 0,
            CorrelationContextHeader = "x-correlation-context",
            CorrelationIdHeader = "x-correlation-id"
        };
        var sut = new GenocsHttpClient(
            httpClient,
            options,
            new SystemTextJsonHttpClientSerializer(),
            new StaticCorrelationContextFactory("tenant=alpha"),
            new StaticCorrelationIdFactory("corr-123"));

        await sut.GetAsync("https://example.test/resource");

        Assert.NotNull(handler.LastHeaders);
        Assert.Equal(["tenant=alpha"], handler.LastHeaders!.CorrelationContextValues);
        Assert.Equal(["corr-123"], handler.LastHeaders.CorrelationIdValues);
    }
}