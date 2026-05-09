namespace Genocs.Library.Demo.WebApi.Features;

public static class HomeFeature
{
    public static IEndpointRouteBuilder MapHomeFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder homeGroup = endpoints.MapGroup("/").WithTags("Home");

        homeGroup.MapGet(string.Empty, () => "Welcome to Genocs Library Demo Web Api!");
        homeGroup.MapGet("/free", () => "this is a free endpoint");

        homeGroup.MapGet("/protected", () => "this is a protected endpoint")
            .RequireAuthorization();

        homeGroup.MapGet("/onlyreader", () => "ok").RequireAuthorization("Reader");
        homeGroup.MapGet("/onlyreader2", () => "ok").RequireAuthorization("Reader2");

        return endpoints;
    }
}
