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

#if NET6_0 || NET7_0 || NET8_0
    ISwaggerOptionsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2);
#endif
    SwaggerOptions Build();
}