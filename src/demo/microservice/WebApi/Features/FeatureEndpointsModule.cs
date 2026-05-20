namespace Genocs.Library.Demo.WebApi.Features;

public static class FeatureEndpointsModule
{
    public static IEndpointRouteBuilder MapFeatures(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHomeFeature();
        endpoints.MapSagaFeature();
        endpoints.MapBookStoreFeature();
        endpoints.MapMongoDbRepositoryFeature();

        return endpoints;
    }
}
