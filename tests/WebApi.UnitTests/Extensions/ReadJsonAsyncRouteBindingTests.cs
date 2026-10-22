using Genocs.WebApi;
using Genocs.WebApi.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Open.Serialization.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Genocs.WebApi.UnitTests.Extensions;

public class ReadJsonAsyncRouteBindingTests
{
    private static readonly IJsonSerializer Serializer = new Open.Serialization.Json.System.JsonSerializerFactory(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    }).GetSerializer();

    [Fact]
    public async Task ReadJsonAsync_BindsRouteValue_WhenConfigured()
    {
        using ServiceProvider provider = CreateProvider(bindRequestFromRoute: true);
        var context = CreateHttpContext(provider, "body-id", "route-id");

        RouteMergeRequest? payload = await context.ReadJsonAsync<RouteMergeRequest>();

        Assert.NotNull(payload);
        Assert.Equal("route-id", payload!.Id);
        Assert.Equal("payload-name", payload.Name);
    }

    [Fact]
    public async Task ReadJsonAsync_DoesNotBindRouteValue_WhenDisabled()
    {
        using ServiceProvider provider = CreateProvider(bindRequestFromRoute: false);
        var context = CreateHttpContext(provider, "body-id", "route-id");

        RouteMergeRequest? payload = await context.ReadJsonAsync<RouteMergeRequest>();

        Assert.NotNull(payload);
        Assert.Equal("body-id", payload!.Id);
        Assert.Equal("payload-name", payload.Name);
    }

    [Fact]
    public async Task ReadJsonAsync_UsesPerHostOptions_WithoutCrossHostLeakage()
    {
        using ServiceProvider enabledProvider = CreateProvider(bindRequestFromRoute: true);
        using ServiceProvider disabledProvider = CreateProvider(bindRequestFromRoute: false);

        var enabledContext = CreateHttpContext(enabledProvider, "body-enabled", "route-enabled");
        var disabledContext = CreateHttpContext(disabledProvider, "body-disabled", "route-disabled");

        RouteMergeRequest? enabledPayload = await enabledContext.ReadJsonAsync<RouteMergeRequest>();
        RouteMergeRequest? disabledPayload = await disabledContext.ReadJsonAsync<RouteMergeRequest>();

        Assert.NotNull(enabledPayload);
        Assert.NotNull(disabledPayload);
        Assert.Equal("route-enabled", enabledPayload!.Id);
        Assert.Equal("body-disabled", disabledPayload!.Id);
    }

    private static ServiceProvider CreateProvider(bool bindRequestFromRoute)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Serializer);
        services.AddSingleton(new WebApiOptions
        {
            BindRequestFromRoute = bindRequestFromRoute
        });

        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext(ServiceProvider provider, string bodyId, string routeId)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };

        string body = $"{{\"id\":\"{bodyId}\",\"name\":\"payload-name\"}}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.RouteValues["id"] = routeId;

        return context;
    }

    private sealed class RouteMergeRequest
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;
    }
}
