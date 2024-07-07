namespace Genocs.WebApi.Swagger.Docs.Configurations;

public interface ISwaggerOptionsBuilder
{
    ISwaggerOptionsBuilder Enable(bool enabled);
    ISwaggerOptionsBuilder ReDocEnable(bool reDocEnabled);
    ISwaggerOptionsBuilder WithName(string name);
    ISwaggerOptionsBuilder WithTitle(string title);
    ISwaggerOptionsBuilder WithVersion(string version);
    ISwaggerOptionsBuilder WithRoutePrefix(string routePrefix);
    ISwaggerOptionsBuilder IncludeSecurity(bool includeSecurity);
    ISwaggerOptionsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2);
    SwaggerSettings Build();
}