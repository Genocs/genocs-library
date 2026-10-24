using System.Net;
using Genocs.Core.Builders;
using Genocs.Http.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class CorrelationHeadersTests
{
    private const string CorrelationContextHeaderName = "x-correlation-context";
    private const string CorrelationIdHeaderName = "x-correlation-id";

    private sealed class StaticCorrelationContextFactory(string? value) : ICorrelationContextFactory
    {
        public string? Create() => value;
    }

    private sealed class StaticCorrelationIdFactory(string? value) : ICorrelationIdFactory
    {
        public string? Create() => value;
    }

    private sealed class SequenceCorrelationIdFactory(params string?[] values) : ICorrelationIdFactory
    {
        private readonly Queue<string?> _values = new(values);

        public string? Create()
            => _values.Count > 0 ? _values.Dequeue() : null;
    }

    private sealed class CapturingHeadersHandler : HttpMessageHandler
    {
        public List<HttpRequestHeadersSnapshot> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new HttpRequestHeadersSnapshot(
                request.Headers.TryGetValues(CorrelationContextHeaderName, out var correlationContextValues)
                    ? correlationContextValues.ToArray()
                    : [],
                request.Headers.TryGetValues(CorrelationIdHeaderName, out var correlationIdValues)
                    ? correlationIdValues.ToArray()
                    : []));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed record HttpRequestHeadersSnapshot(string[] CorrelationContextValues, string[] CorrelationIdValues);

    private sealed class TestClientContext : IDisposable
    {
        public TestClientContext(ServiceProvider provider)
        {
            Provider = provider;
            Client = provider.GetRequiredService<IHttpClient>();
        }

        public ServiceProvider Provider { get; }
        public IHttpClient Client { get; }

        public void Dispose() => Provider.Dispose();
    }

    private static IGenocsBuilder CreateBuilder(IServiceCollection services, IConfiguration configuration)
        => GenocsBuilder.Create(services, configuration);

    private static IConfiguration CreateCorrelationHeaderConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["httpClient:correlationContextHeader"] = CorrelationContextHeaderName,
                ["httpClient:correlationIdHeader"] = CorrelationIdHeaderName,
            })
            .Build();

    private static TestClientContext CreateClient(
        CapturingHeadersHandler capturingHandler,
        ICorrelationContextFactory correlationContextFactory,
        ICorrelationIdFactory correlationIdFactory)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICorrelationContextFactory>(correlationContextFactory);
        services.AddSingleton<ICorrelationIdFactory>(correlationIdFactory);

        var builder = CreateBuilder(services, CreateCorrelationHeaderConfiguration());
        builder.AddHttpClient(httpClientBuilder: clientBuilder =>
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() => capturingHandler));

        return new TestClientContext(services.BuildServiceProvider());
    }

    [Fact]
    public async Task Null_or_whitespace_correlation_values_are_not_added_as_headers()
    {
        using var handler = new CapturingHeadersHandler();
        using var context = CreateClient(
            handler,
            new StaticCorrelationContextFactory(null),
            new StaticCorrelationIdFactory("   "));

        await context.Client.GetAsync("https://example.test/resource");

        Assert.Single(handler.Requests);
        Assert.Empty(handler.Requests[0].CorrelationContextValues);
        Assert.Empty(handler.Requests[0].CorrelationIdValues);
    }

    [Fact]
    public async Task Non_empty_correlation_values_are_added_as_headers()
    {
        using var handler = new CapturingHeadersHandler();
        using var context = CreateClient(
            handler,
            new StaticCorrelationContextFactory("tenant=alpha"),
            new StaticCorrelationIdFactory("corr-123"));

        await context.Client.GetAsync("https://example.test/resource");

        Assert.Single(handler.Requests);
        Assert.Equal(["tenant=alpha"], handler.Requests[0].CorrelationContextValues);
        Assert.Equal(["corr-123"], handler.Requests[0].CorrelationIdValues);
    }

    [Fact]
    public async Task Caller_supplied_correlation_headers_are_not_overwritten()
    {
        using var handler = new CapturingHeadersHandler();
        using var context = CreateClient(
            handler,
            new StaticCorrelationContextFactory("factory-context"),
            new StaticCorrelationIdFactory("factory-id"));

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/resource");
        request.Headers.TryAddWithoutValidation(CorrelationContextHeaderName, "caller-context");
        request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, "caller-id");

        await context.Client.SendAsync(request);

        Assert.Single(handler.Requests);
        Assert.Equal(["caller-context"], handler.Requests[0].CorrelationContextValues);
        Assert.Equal(["caller-id"], handler.Requests[0].CorrelationIdValues);
    }

    [Fact]
    public async Task Correlation_values_are_evaluated_for_each_request()
    {
        using var handler = new CapturingHeadersHandler();
        using var context = CreateClient(
            handler,
            new StaticCorrelationContextFactory("tenant=alpha"),
            new SequenceCorrelationIdFactory("corr-1", "corr-2"));

        await context.Client.GetAsync("https://example.test/one");
        await context.Client.GetAsync("https://example.test/two");

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(["corr-1"], handler.Requests[0].CorrelationIdValues);
        Assert.Equal(["corr-2"], handler.Requests[1].CorrelationIdValues);
    }
}