using System.Net;
using Genocs.Http.Configurations;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class RequestUriTests
{
    private sealed class StubCorrelationContextFactory : ICorrelationContextFactory
    {
        public string Create() => string.Empty;
    }

    private sealed class StubCorrelationIdFactory : ICorrelationIdFactory
    {
        public string Create() => string.Empty;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ClientWithUriParse : GenocsHttpClient
    {
        public ClientWithUriParse(
            HttpClient client,
            HttpClientOptions settings,
            IHttpClientSerializer serializer,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationIdFactory correlationIdFactory)
            : base(client, settings, serializer, correlationContextFactory, correlationIdFactory)
        {
        }

        public static Uri Parse(string uri) => ParseRequestUri(uri);
    }

    private static GenocsHttpClient CreateClient(HttpClient httpClient)
    {
        var options = new HttpClientOptions { Retries = 0 };
        return new GenocsHttpClient(
            httpClient,
            options,
            new SystemTextJsonHttpClientSerializer(),
            new StubCorrelationContextFactory(),
            new StubCorrelationIdFactory());
    }

    [Fact]
    public async Task Relative_uri_combines_with_BaseAddress()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com/api/", UriKind.Absolute)
        };
        var sut = CreateClient(httpClient);

        await sut.GetAsync("items/7");

        Assert.NotNull(handler.LastRequestUri);
        Assert.Equal("https://api.example.com/api/items/7", handler.LastRequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task Absolute_uri_is_not_combined_with_BaseAddress()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ignored.example/", UriKind.Absolute)
        };
        var sut = CreateClient(httpClient);

        await sut.GetAsync("https://api.example.com/v2/status");

        Assert.NotNull(handler.LastRequestUri);
        Assert.Equal("https://api.example.com/v2/status", handler.LastRequestUri!.AbsoluteUri);
    }

    [Fact]
    public void ParseRequestUri_rejects_invalid_string()
    {
        // Fails System.Uri parsing (invalid percent-encoding in authority/path).
        var ex = Assert.Throws<ArgumentException>(() => ClientWithUriParse.Parse("http://%zz"));
        Assert.Equal("uri", ex.ParamName);
    }

    [Fact]
    public void ParseRequestUri_rejects_whitespace()
    {
        Assert.Throws<ArgumentException>(() => ClientWithUriParse.Parse("   "));
    }

    [Fact]
    public async Task GetAsync_throws_for_invalid_uri_before_send()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var sut = CreateClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.GetAsync("http://%zz"));

        Assert.Null(handler.LastRequestUri);
    }
}
