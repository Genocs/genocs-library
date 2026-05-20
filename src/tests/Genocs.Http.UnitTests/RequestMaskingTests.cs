using System.Collections.Concurrent;
using System.Net;
using Genocs.Core.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class RequestMaskingTests
{
    private sealed class CapturingLogs
    {
        private readonly ConcurrentQueue<string> _messages = [];

        public IReadOnlyCollection<string> Messages => _messages;

        public ILoggerProvider CreateProvider() => new Provider(_messages);

        private sealed class Provider(ConcurrentQueue<string> messages) : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => new Logger(messages);

            public void Dispose()
            {
            }
        }

        private sealed class Logger(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => messages.Enqueue(formatter(state, exception));
        }
    }

    private sealed class OkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
    }

    private sealed class TestClientContext : IDisposable
    {
        public TestClientContext(ServiceProvider provider, CapturingLogs logs)
        {
            Provider = provider;
            Logs = logs;
            Client = provider.GetRequiredService<IHttpClient>();
        }

        public ServiceProvider Provider { get; }
        public CapturingLogs Logs { get; }
        public IHttpClient Client { get; }

        public void Dispose() => Provider.Dispose();
    }

    private static TestClientContext CreateClient(
        IEnumerable<string> urlParts,
        string maskTemplate,
        HttpMessageHandler primaryHandler)
    {
        var logs = new CapturingLogs();
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(logs.CreateProvider());
            builder.SetMinimumLevel(LogLevel.Information);
        });

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["httpClient:requestMasking:enabled"] = "true",
                ["httpClient:requestMasking:maskTemplate"] = maskTemplate,
            })
            .Build();

        var builder = GenocsBuilder.Create(services, configuration);
        builder.AddHttpClient(
            maskedRequestUrlParts: urlParts,
            httpClientBuilder: clientBuilder =>
                clientBuilder.ConfigurePrimaryHttpMessageHandler(() => primaryHandler));

        return new TestClientContext(services.BuildServiceProvider(), logs);
    }

    [Fact]
    public async Task Request_masking_masks_all_exact_token_occurrences_in_logged_uri()
    {
        using var context = CreateClient(
            ["secret", "token=abc"],
            "*****",
            new OkHandler());

        await context.Client.GetAsync("https://api.test/orders/secret?token=abc&debug=secret");

        string startLog = Assert.Single(context.Logs.Messages.Where(message =>
            message.Contains("Start processing Http request", StringComparison.Ordinal)));

        Assert.Contains("https://api.test/orders/*****?*****&debug=*****", startLog, StringComparison.Ordinal);
        Assert.DoesNotContain("token=abc", startLog, StringComparison.Ordinal);
        Assert.DoesNotContain("/secret", startLog, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Request_masking_does_not_throw_when_mask_template_is_not_uri_safe()
    {
        using var context = CreateClient(
            ["secret"],
            "% not-a-uri %",
            new OkHandler());

        var response = await context.Client.GetAsync("https://api.test/items/secret?note=secret");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string startLog = Assert.Single(context.Logs.Messages.Where(message =>
            message.Contains("Start processing Http request", StringComparison.Ordinal)));
        Assert.Contains("https://api.test/items/% not-a-uri %?note=% not-a-uri %", startLog, StringComparison.Ordinal);
    }
}