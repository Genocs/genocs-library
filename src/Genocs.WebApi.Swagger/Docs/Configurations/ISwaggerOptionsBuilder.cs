namespace Genocs.WebApi.Swagger.Docs.Configurations;
public interface ISwaggerOptionsBuilder
{
    ISwaggerOptionsBuilder Enable(bool enabled);
    ISwaggerOptionsBuilder ReDocEnable(bool reDocEnabled);
    ISwaggerOptionsBuilder WithName(string name);
    ISwaggerOptionsBuilder WithTitle(string title);
    ISwaggerOptionsBuilder WithVersion(string version);
    ISwaggerOptionsBuilder WithDescription(string description);
    ISwaggerOptionsBuilder WithRoutePrefix(string routePrefix);
    ISwaggerOptionsBuilder WithContactName(string contactName);
    ISwaggerOptionsBuilder IncludeSecurity(bool includeSecurity);
    ISwaggerOptionsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2);
    SwaggerOptions Build();
}