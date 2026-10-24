using Genocs.Core.Builders;
using Genocs.Telemetry;
using Genocs.Telemetry.Configurations;
using Genocs.Telemetry.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Telemetry.UnitTests;

public class PrometheusUnitTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizePrometheusEndpoint_WhenEndpointIsBlank_ReturnsDefault(string? endpoint)
    {
        string normalized = OpenTelemetryExtensions.NormalizePrometheusEndpoint(endpoint);

        Assert.Equal(PrometheusOptions.DefaultEndpoint, normalized);
    }

    [Theory]
    [InlineData("metrics", "/metrics")]
    [InlineData("custom-metrics", "/custom-metrics")]
    [InlineData("scrape/here", "/scrape/here")]
    public void NormalizePrometheusEndpoint_WhenEndpointMissingLeadingSlash_AddsSlash(string endpoint, string expected)
    {
        string normalized = OpenTelemetryExtensions.NormalizePrometheusEndpoint(endpoint);

        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("/metrics")]
    [InlineData("/scrape/custom")]
    public void NormalizePrometheusEndpoint_WhenEndpointHasLeadingSlash_PreservesValue(string endpoint)
    {
        string normalized = OpenTelemetryExtensions.NormalizePrometheusEndpoint(endpoint);

        Assert.Equal(endpoint, normalized);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenPathDoesNotMatchEndpoint_AlwaysCallsNext()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            ApiKey = "secret"
        });

        var context = CreateHttpContext(path: "/healthz");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
        Assert.NotEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenNoApiKeyAndNoAllowedHosts_AllowsScrape()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true
        });

        var context = CreateHttpContext(path: "/metrics");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenApiKeyConfiguredAndProvided_AllowsScrape()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            ApiKey = "expected-key"
        });

        var context = CreateHttpContext(path: "/metrics", queryString: "?apiKey=expected-key");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-key")]
    public async Task PrometheusMiddleware_WhenApiKeyConfiguredAndMissingOrWrong_Returns404(string? providedKey)
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            ApiKey = "expected-key"
        });

        string queryString = providedKey is null ? string.Empty : $"?apiKey={providedKey}";
        var context = CreateHttpContext(path: "/metrics", queryString: queryString);

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextWasCalled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenAllowedHostMatchesRequestHost_AllowsScrape()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            AllowedHosts = ["scraper.local"]
        });

        var context = CreateHttpContext(path: "/metrics", host: "scraper.local");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenAllowedHostMatchesForwardedFor_AllowsScrape()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            AllowedHosts = ["10.0.0.5"]
        });

        var context = CreateHttpContext(
            path: "/metrics",
            host: "internal-loadbalancer",
            forwardedFor: "10.0.0.5");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextWasCalled);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenAllowedHostsConfiguredAndNoMatch_Returns404()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            AllowedHosts = ["scraper.local"]
        });

        var context = CreateHttpContext(path: "/metrics", host: "attacker.example.com");

        bool nextWasCalled = false;
        await middleware.InvokeAsync(context, _ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextWasCalled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenApiKeyAndAllowedHostsConfigured_PassesOnEither()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            ApiKey = "k",
            AllowedHosts = ["scraper.local"]
        });

        var contextWithKey = CreateHttpContext(path: "/metrics", queryString: "?apiKey=k", host: "anywhere");
        var contextWithHost = CreateHttpContext(path: "/metrics", host: "scraper.local");

        int nextCallCount = 0;
        Task IncrementNext(HttpContext _)
        {
            Interlocked.Increment(ref nextCallCount);
            return Task.CompletedTask;
        }

        await middleware.InvokeAsync(contextWithKey, IncrementNext);
        await middleware.InvokeAsync(contextWithHost, IncrementNext);

        Assert.Equal(2, nextCallCount);
    }

    [Fact]
    public async Task PrometheusMiddleware_WhenCustomEndpointConfigured_GuardsConfiguredPathOnly()
    {
        var middleware = CreateMiddleware(new PrometheusOptions
        {
            Enabled = true,
            Endpoint = "scrape",
            ApiKey = "expected-key"
        });

        var contextOnEndpoint = CreateHttpContext(path: "/scrape");
        var contextOnDefaultMetrics = CreateHttpContext(path: "/metrics");

        bool guardedNextWasCalled = false;
        await middleware.InvokeAsync(contextOnEndpoint, _ =>
        {
            guardedNextWasCalled = true;
            return Task.CompletedTask;
        });

        bool unguardedNextWasCalled = false;
        await middleware.InvokeAsync(contextOnDefaultMetrics, _ =>
        {
            unguardedNextWasCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(guardedNextWasCalled);
        Assert.Equal(StatusCodes.Status404NotFound, contextOnEndpoint.Response.StatusCode);

        Assert.True(unguardedNextWasCalled);
    }

    [Fact]
    public void AddTelemetry_WhenPrometheusEnabled_RegistersOptionsAndMiddleware()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(prometheusEnabled: true);

        IGenocsBuilder builder = services.AddGenocs(configuration);
        builder.AddTelemetry();

        using ServiceProvider provider = services.BuildServiceProvider();

        PrometheusOptions? options = provider.GetService<PrometheusOptions>();
        Assert.NotNull(options);
        Assert.True(options!.Enabled);

        PrometheusMiddleware? middleware = provider.GetService<PrometheusMiddleware>();
        Assert.NotNull(middleware);
    }

    [Fact]
    public void AddTelemetry_WhenPrometheusDisabled_DoesNotRegisterPrometheusOptions()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(prometheusEnabled: false);

        IGenocsBuilder builder = services.AddGenocs(configuration);
        builder.AddTelemetry();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<PrometheusOptions>());
        Assert.Null(provider.GetService<PrometheusMiddleware>());
    }

    [Fact]
    public void UsePrometheus_WhenPrometheusOptionsNotRegistered_IsNoOp()
    {
        IServiceCollection services = new ServiceCollection();
        using ServiceProvider provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);

        IApplicationBuilder result = app.UsePrometheus();

        Assert.Same(app, result);
    }

    [Fact]
    public void UsePrometheus_WhenPrometheusDisabled_IsNoOp()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new PrometheusOptions { Enabled = false });
        using ServiceProvider provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);

        IApplicationBuilder result = app.UsePrometheus();

        Assert.Same(app, result);
    }

    [Fact]
    public void MapPrometheus_WhenPrometheusDisabled_IsNoOp()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new PrometheusOptions { Enabled = false });
        using ServiceProvider provider = services.BuildServiceProvider();

        var endpoints = new TestEndpointRouteBuilder(provider);

        IEndpointRouteBuilder result = endpoints.MapPrometheus();

        Assert.Same(endpoints, result);
        Assert.Empty(endpoints.DataSources);
    }

    private static PrometheusMiddleware CreateMiddleware(PrometheusOptions options)
        => new PrometheusMiddleware(options);

    private static DefaultHttpContext CreateHttpContext(
        string path,
        string? queryString = null,
        string? host = null,
        string? forwardedFor = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        if (!string.IsNullOrEmpty(queryString))
        {
            context.Request.QueryString = new QueryString(queryString);
        }

        if (!string.IsNullOrEmpty(host))
        {
            context.Request.Host = new HostString(host);
        }

        if (!string.IsNullOrEmpty(forwardedFor))
        {
            context.Request.Headers["x-forwarded-for"] = forwardedFor;
        }

        return context;
    }

    private static IConfiguration CreateConfiguration(bool prometheusEnabled)
    {
        Dictionary<string, string?> settings = new()
        {
            ["app:service"] = "telemetry-prometheus-tests",
            ["telemetry:enabled"] = "true",
            ["telemetry:exporter:enabled"] = "false",
            ["telemetry:console:enabled"] = "false",
            ["telemetry:azure:enabled"] = "false",
            ["telemetry:prometheus:enabled"] = prometheusEnabled ? "true" : "false"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private sealed class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public TestEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DataSources = new List<EndpointDataSource>();
        }

        public IServiceProvider ServiceProvider { get; }

        public ICollection<EndpointDataSource> DataSources { get; }

        public IApplicationBuilder CreateApplicationBuilder() => new ApplicationBuilder(ServiceProvider);
    }
}
