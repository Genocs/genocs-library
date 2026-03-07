using Microsoft.AspNetCore.Routing;

namespace Genocs.Library.Demo.WebApi.Features;

public static class FeatureEndpointsModule
{
    public static IEndpointRouteBuilder MapFeatures(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHomeFeature();
        endpoints.MapSagaFeature();

        return endpoints;
    }
}
