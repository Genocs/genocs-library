namespace Genocs.WebApi.Swagger.Configurations;
public interface IOpenApiSettingsBuilder
{
    IOpenApiSettingsBuilder Enable(bool enabled);
    IOpenApiSettingsBuilder ReDocEnable(bool reDocEnabled);
    IOpenApiSettingsBuilder WithName(string name);
    IOpenApiSettingsBuilder WithTitle(string title);
    IOpenApiSettingsBuilder WithVersion(string version);
    IOpenApiSettingsBuilder WithRoutePrefix(string routePrefix);
    IOpenApiSettingsBuilder IncludeSecurity(bool includeSecurity);
    IOpenApiSettingsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2);
    OpenApiSettings Build();
}