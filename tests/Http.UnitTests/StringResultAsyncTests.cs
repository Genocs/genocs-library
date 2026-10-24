using System.Net;
using Genocs.Http.Configurations;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class StringResultAsyncTests
{
    private sealed class StubCorrelationContextFactory : ICorrelationContextFactory
    {
        public string? Create() => string.Empty;
    }

    private sealed class StubCorrelationIdFactory : ICorrelationIdFactory
    {
        public string? Create() => string.Empty;
    }

    private sealed class FixedStatusHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public FixedStatusHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private static GenocsHttpClient CreateClient(HttpMessageHandler handler, int retries = 0)
    {
        var options = new HttpClientOptions { Retries = retries };
        var httpClient = new HttpClient(handler);
        return new GenocsHttpClient(
            httpClient,
            options,
            new SystemTextJsonHttpClientSerializer(),
            new StubCorrelationContextFactory(),
            new StubCorrelationIdFactory());
    }

    [Fact]
    public async Task GetResultAsync_does_not_throw_on_non_success_status()
    {
        using var handler = new FixedStatusHandler(HttpStatusCode.NotFound);
        var sut = CreateClient(handler);

        var result = await sut.GetResultAsync<System.Text.Json.JsonElement>("https://example.test/api/r");

        Assert.Equal(HttpStatusCode.NotFound, result.Response.StatusCode);
    }

    [Fact]
    public async Task PostResultAsync_does_not_throw_on_non_success_status()
    {
        using var handler = new FixedStatusHandler(HttpStatusCode.BadRequest);
        var sut = CreateClient(handler);

        var result = await sut.PostResultAsync<System.Text.Json.JsonElement>("https://example.test/api/r", new { x = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
    }
}
