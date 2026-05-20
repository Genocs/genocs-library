using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Core.UnitTests.Builders;

public class MapDefaultEndpointsTests
{
    [Fact]
    public async Task Root_ReturnsServiceName_WhenAppOptionsNameConfigured()
    {
        using IHost host = await BuildHostAsync(services =>
            services.AddSingleton(new AppOptions { Name = "BookStore" }));

        using HttpClient client = host.GetTestClient();
        string response = await client.GetStringAsync("/");

        Assert.Equal("BookStore", response);
    }

    [Fact]
    public async Task Root_ReturnsFallbackMessage_WhenAppOptionsNameAbsent()
    {
        using IHost host = await BuildHostAsync(services =>
            services.AddSingleton(new AppOptions { Name = null }));

        using HttpClient client = host.GetTestClient();
        string response = await client.GetStringAsync("/");

        // With no service name the response is the computed "Service <version> is running" message.
        Assert.StartsWith("Service", response);
        Assert.Contains("is running", response);
    }

    [Fact]
    public async Task Root_ReturnsFallbackMessage_WhenNoAppOptionsRegistered()
    {
        using IHost host = await BuildHostAsync(_ => { });

        using HttpClient client = host.GetTestClient();
        string response = await client.GetStringAsync("/");

        Assert.StartsWith("Service", response);
        Assert.Contains("is running", response);
    }

    [Fact]
    public async Task Healthz_RespondsSuccessfully()
    {
        using IHost host = await BuildHostAsync(_ => { });

        using HttpClient client = host.GetTestClient();
        HttpResponseMessage response = await client.GetAsync("/healthz");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Alive_RespondsSuccessfully()
    {
        using IHost host = await BuildHostAsync(_ => { });

        using HttpClient client = host.GetTestClient();
        HttpResponseMessage response = await client.GetAsync("/alive");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Endpoints_AreNotMapped_InNonDevelopmentEnvironment()
    {
        using IHost host = await BuildHostAsync(_ => { }, Environments.Production);

        using HttpClient client = host.GetTestClient();

        HttpResponseMessage rootResponse = await client.GetAsync("/");
        HttpResponseMessage healthzResponse = await client.GetAsync("/healthz");
        HttpResponseMessage aliveResponse = await client.GetAsync("/alive");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, rootResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, healthzResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, aliveResponse.StatusCode);
    }

    private static async Task<IHost> BuildHostAsync(Action<IServiceCollection> configureServices, string environmentName = "Development")
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseEnvironment(environmentName);
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.MapDefaultEndpoints();
                });
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddHealthChecks();
                    configureServices(services);
                });
            })
            .Build();

        await host.StartAsync();
        return host;
    }
}
