using System.Net;
using System.Net.Http.Json;
using System.Text;
using Genocs.Logging;
using Genocs.Logging.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Logging.IntegrationTests;

[CollectionDefinition(nameof(LoggingIntegrationCollection), DisableParallelization = true)]
public sealed class LoggingIntegrationCollection;

[Collection(nameof(LoggingIntegrationCollection))]
public sealed class LoggingHostIntegrationTests
{
    [Fact]
    public async Task MapLogLevelHandler_WithMissingLevel_ReturnsBadRequestJson()
    {
        await using var app = await CreateAppAsync(configureApp: webApp =>
        {
            webApp.MapLogLevelHandler();
        });

        using HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.PostAsync("/logging/level", content: null);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Missing 'level' query parameter", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MapLogLevelHandler_WithValidLevel_ReturnsOkJson()
    {
        await using var app = await CreateAppAsync(configureApp: webApp =>
        {
            webApp.MapLogLevelHandler();
        });

        using HttpClient client = app.GetTestClient();

        HttpResponseMessage response = await client.PostAsync("/logging/level?level=Debug", content: null);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("{\"level\":\"Debug\"}", body);
    }

    [Fact]
    public async Task CorrelationContextLoggingMiddleware_WithPayloadCaptureEnabled_PreservesRequestAndResponseFlow()
    {
        await using var app = await CreateAppAsync(
            configureServices: services =>
            {
                services.AddTransient<CorrelationContextLoggingMiddleware>();
                services.AddSingleton(new LoggerOptions
                {
                    HttpPayload = new HttpPayloadOptions
                    {
                        Enabled = true,
                        CaptureRequestBody = true,
                        CaptureResponseBody = true,
                        AllowedContentTypes = ["application/json"],
                        MaxBodyLength = 4096
                    }
                });
            },
            configureApp: webApp =>
            {
                webApp.UseCorrelationContextLogging();
                webApp.MapPost("/echo", async context =>
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                    string payload = await reader.ReadToEndAsync();

                    context.Response.ContentType = "application/json";
                    context.Response.Headers["X-Request-Length"] = payload.Length.ToString();
                    await context.Response.WriteAsync(payload);
                });
            });

        using HttpClient client = app.GetTestClient();

        const string payload = "{\"message\":\"integration\"}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/echo")
        {
            Content = JsonContent.Create(new { message = "integration" })
        };

        HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(payload, body);
        Assert.True(response.Headers.TryGetValues("X-Request-Length", out var values));
        Assert.Equal(payload.Length.ToString(), values.Single());
    }

    private static async Task<WebApplication> CreateAppAsync(
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configureApp = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["app:service"] = "logging-integration-tests",
            ["app:version"] = "1.0.0",
            ["logger:enabled"] = "true",
            ["logger:console:enabled"] = "false",
            ["logger:file:enabled"] = "false",
            ["logger:seq:enabled"] = "false",
            ["logger:loki:enabled"] = "false",
            ["logger:elk:enabled"] = "false",
            ["logger:azure:enabled"] = "false"
        });

        builder.Host.UseLogging();
        configureServices?.Invoke(builder.Services);

        WebApplication app = builder.Build();
        configureApp?.Invoke(app);

        await app.StartAsync();
        return app;
    }
}
