namespace Genocs.WebApi.OpenApi.Configurations;
public interface IOpenApiOptionsBuilder
{
    IOpenApiOptionsBuilder Enable(bool enabled);
    IOpenApiOptionsBuilder ReDocEnable(bool reDocEnabled);
    IOpenApiOptionsBuilder WithName(string name);
    IOpenApiOptionsBuilder WithTitle(string title);
    IOpenApiOptionsBuilder WithVersion(string version);
    IOpenApiOptionsBuilder WithDescription(string description);
    IOpenApiOptionsBuilder WithRoutePrefix(string routePrefix);
    IOpenApiOptionsBuilder WithContactName(string contactName);
    IOpenApiOptionsBuilder IncludeSecurity(bool includeSecurity);
    OpenApiOptions Build();
}