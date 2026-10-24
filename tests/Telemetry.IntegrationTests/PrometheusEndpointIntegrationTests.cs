using System.Net;
using Genocs.Core.Builders;
using Genocs.Telemetry;
using Genocs.Telemetry.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Telemetry.IntegrationTests;

[Collection(nameof(TelemetryIntegrationCollection))]
public sealed class PrometheusEndpointIntegrationTests
{
    [Fact]
    public async Task Prometheus_WhenEnabledWithoutGuards_ScrapeEndpointReturnsOk()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenDisabled_ScrapeEndpointReturnsNotFound()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "false"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenApiKeyConfiguredAndMissing_ReturnsNotFound()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true",
            ["telemetry:prometheus:apiKey"] = "expected-key"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenApiKeyConfiguredAndProvided_ReturnsOk()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true",
            ["telemetry:prometheus:apiKey"] = "expected-key"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics?apiKey=expected-key"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenAllowedHostsConfiguredAndForwardedForMatches_ReturnsOk()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true",
            ["telemetry:prometheus:allowedHosts:0"] = "10.0.0.5"
        });

        HttpClient client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("x-forwarded-for", "10.0.0.5");

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenAllowedHostsConfiguredAndCallerIsNotAllowed_ReturnsNotFound()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true",
            ["telemetry:prometheus:allowedHosts:0"] = "10.0.0.5"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Prometheus_WhenCustomEndpointConfigured_MapsCustomPath()
    {
        await using WebApplication app = await BuildAppAsync(new()
        {
            ["telemetry:prometheus:enabled"] = "true",
            ["telemetry:prometheus:endpoint"] = "scrape"
        });

        HttpClient client = app.GetTestClient();

        HttpResponseMessage scrapeResponse = await client.GetAsync(new Uri("http://localhost/scrape"));
        HttpResponseMessage defaultResponse = await client.GetAsync(new Uri("http://localhost/metrics"));

        Assert.Equal(HttpStatusCode.OK, scrapeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, defaultResponse.StatusCode);
    }

    private static async Task<WebApplication> BuildAppAsync(Dictionary<string, string?> overrides)
    {
        Dictionary<string, string?> settings = new()
        {
            ["app:service"] = "telemetry-prometheus-integration-tests",
            ["telemetry:enabled"] = "true",
            ["telemetry:exporter:enabled"] = "false",
            ["telemetry:console:enabled"] = "false",
            ["telemetry:azure:enabled"] = "false"
        };

        foreach (KeyValuePair<string, string?> entry in overrides)
        {
            settings[entry.Key] = entry.Value;
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.Configuration.AddInMemoryCollection(settings);
        builder.WebHost.UseTestServer();

        IGenocsBuilder genocsBuilder = builder.AddGenocs();
        genocsBuilder.AddTelemetry();

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UsePrometheus();

        IEndpointRouteBuilder endpointBuilder = (IEndpointRouteBuilder)app;
        endpointBuilder.MapPrometheus();

        await app.StartAsync();
        return app;
    }
}
