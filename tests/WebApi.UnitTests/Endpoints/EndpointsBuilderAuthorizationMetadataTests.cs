using Genocs.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Genocs.WebApi.UnitTests.Endpoints;

public class EndpointsBuilderAuthorizationMetadataTests
{
    [Fact]
    public void DefaultEndpoint_DoesNotAddAuthorizationOrAnonymousMetadata()
    {
        using var app = CreateApp();
        var definitions = new WebApiEndpointDefinitions();

        IEndpointsBuilder sut = new EndpointsBuilder(app, definitions);
        sut.Get("/public");

        RouteEndpoint endpoint = GetRouteEndpoint(app, "/public", HttpMethods.Get);

        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Empty(endpoint.Metadata.OfType<IAuthorizeData>());
    }

    [Fact]
    public void AuthTrue_AddsAuthorizationMetadata()
    {
        using var app = CreateApp();
        var definitions = new WebApiEndpointDefinitions();

        IEndpointsBuilder sut = new EndpointsBuilder(app, definitions);
        sut.Get("/secure", auth: true);

        RouteEndpoint endpoint = GetRouteEndpoint(app, "/secure", HttpMethods.Get);

        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    [Fact]
    public void Roles_AddsRoleBasedAuthorizationMetadata()
    {
        using var app = CreateApp();
        var definitions = new WebApiEndpointDefinitions();

        IEndpointsBuilder sut = new EndpointsBuilder(app, definitions);
        sut.Get("/roles", roles: "admin,manager");

        RouteEndpoint endpoint = GetRouteEndpoint(app, "/roles", HttpMethods.Get);
        IAuthorizeData authorizeData = Assert.Single(endpoint.Metadata.OfType<IAuthorizeData>());

        Assert.Equal("admin,manager", authorizeData.Roles);
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    [Fact]
    public void Policies_AddsPolicyAuthorizationMetadata()
    {
        using var app = CreateApp();
        var definitions = new WebApiEndpointDefinitions();

        IEndpointsBuilder sut = new EndpointsBuilder(app, definitions);
        sut.Get("/policy", policies: ["p1", "p2"]);

        RouteEndpoint endpoint = GetRouteEndpoint(app, "/policy", HttpMethods.Get);
        var policies = endpoint.Metadata
            .OfType<IAuthorizeData>()
            .Select(d => d.Policy)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .OfType<string>()
            .ToArray();

        Assert.Equal(["p1", "p2"], policies);
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder();
        return builder.Build();
    }

    private static RouteEndpoint GetRouteEndpoint(WebApplication app, string path, string method)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        return routeBuilder.DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint =>
            {
                var httpMethods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;
                return string.Equals(endpoint.RoutePattern.RawText, path, StringComparison.Ordinal)
                    && httpMethods?.Contains(method, StringComparer.OrdinalIgnoreCase) == true;
            });
    }
}
