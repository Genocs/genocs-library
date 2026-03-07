using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace Genocs.Library.Demo.WebApi.Features;

public static class HomeFeature
{
    public static IEndpointRouteBuilder MapHomeFeature(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(string.Empty, () => "Welcome to Genocs Library Demo Web Api!");
        endpoints.MapGet("/free", () => "this is a free endpoint");

        endpoints.MapGet("/protected", () => "this is a protected endpoint")
            .RequireAuthorization()
            .WithTags("Home");

        endpoints.MapGet("/onlyreader", () => "ok").RequireAuthorization("Reader");
        endpoints.MapGet("/onlyreader2", () => "ok").RequireAuthorization("Reader2");

        return endpoints;
    }
}
