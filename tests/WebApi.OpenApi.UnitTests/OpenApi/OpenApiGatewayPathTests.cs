using System.Text.Json;
using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
using Genocs.WebApi.OpenApi.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.WebApi.OpenApi.UnitTests.OpenApi;

public class OpenApiGatewayPathTests
{
    [Theory]
    [InlineData("swagger", "Notifications", "absolute", "/swagger/Notifications/swagger.json")]
    [InlineData("docs", "v1", "absolute", "/docs/v1/swagger.json")]
    [InlineData("", "v1", "absolute", "/v1/swagger.json")]
    [InlineData("swagger", "Notifications", "relative", "Notifications/swagger.json")]
    [InlineData("docs", "v1", "relative", "v1/swagger.json")]
    [InlineData("", "v1", "relative", "v1/swagger.json")]
    public void SpecUrl_MatchesDocumentPathTable(string routePrefix, string name, string documentPath, string expected)
    {
        Assert.Equal(expected, OpenApiSpecUrl.Build(routePrefix, name, documentPath));
    }

    [Fact]
    public void SpecUrl_RejectsUnknownDocumentPath()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => OpenApiSpecUrl.Build("swagger", "v1", "root"));

        Assert.Contains("absolute", exception.Message, StringComparison.Ordinal);
        Assert.Contains("relative", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SwaggerIndex_UsesRelativeSpecUrl_WhenDocumentPathIsRelative()
    {
        await using WebApplication app = await CreateHostAsync(documentPath: "relative", name: "Notifications");

        string script = await app.GetTestClient().GetStringAsync("/swagger/index.js");

        Assert.Contains("\"url\":\"Notifications/swagger.json\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("\"/swagger/Notifications/swagger.json\"", script, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SwaggerIndex_UsesAbsoluteSpecUrl_WhenDocumentPathIsOmitted()
    {
        await using WebApplication app = await CreateHostAsync(documentPath: null, name: "Notifications");

        string script = await app.GetTestClient().GetStringAsync("/swagger/index.js");

        Assert.Contains("\"/swagger/Notifications/swagger.json\"", script, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Document_UsesForwardedPrefix_WhenOptionIsEnabledAndHeaderIsSafe()
    {
        await using WebApplication app = await CreateHostAsync(
            documentPath: "absolute",
            name: "Notifications",
            useForwardedPrefix: true,
            serverUrl: "http://localhost:5541");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/Notifications/swagger.json");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/notifications-service");

        string body = await (await app.GetTestClient().SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Equal("/notifications-service", ReadFirstServerUrl(body));
    }

    [Fact]
    public async Task Document_IgnoresForwardedPrefix_WhenOptionIsDisabled()
    {
        await using WebApplication app = await CreateHostAsync(
            documentPath: "absolute",
            name: "Notifications",
            useForwardedPrefix: false,
            serverUrl: "http://localhost:5541");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/Notifications/swagger.json");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/notifications-service");

        string body = await (await app.GetTestClient().SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Equal("http://localhost:5541", ReadFirstServerUrl(body));
    }

    [Fact]
    public async Task Document_KeepsConfiguredServers_WhenForwardedPrefixIsNotAPath()
    {
        await using WebApplication app = await CreateHostAsync(
            documentPath: "absolute",
            name: "Notifications",
            useForwardedPrefix: true,
            serverUrl: "http://localhost:5541");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/Notifications/swagger.json");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "https://evil.example");

        string body = await (await app.GetTestClient().SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Equal("http://localhost:5541", ReadFirstServerUrl(body));
    }

    private static async Task<WebApplication> CreateHostAsync(
        string? documentPath,
        string name,
        bool useForwardedPrefix = false,
        string? serverUrl = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();

        var options = new OpenApiOptionsBuilder()
            .Enable(true)
            .ReDocEnable(false)
            .WithName(name)
            .WithTitle("Notifications Service")
            .WithVersion("1.0.0")
            .WithRoutePrefix("swagger");

        if (documentPath is not null)
        {
            options.WithDocumentPath(documentPath);
        }

        options.UseForwardedPrefix(useForwardedPrefix);

        if (serverUrl is not null)
        {
            options.AddServer(serverUrl);
        }

        builder.AddGenocs().AddWebApi().AddOpenApiDocs(options.Build());

        WebApplication app = builder.Build();
        app.UseOpenApiDocs();
        await app.StartAsync();
        return app;
    }

    private static string? ReadFirstServerUrl(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("servers")[0].GetProperty("url").GetString();
    }
}
