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
    IOpenApiOptionsBuilder WithContactEmail(string contactEmail);
    IOpenApiOptionsBuilder WithContactUrl(string contactUrl);
    IOpenApiOptionsBuilder WithLicenseName(string licenseName);
    IOpenApiOptionsBuilder WithLicenseUrl(string licenseUrl);
    IOpenApiOptionsBuilder WithTermsOfService(string termsOfService);
    IOpenApiOptionsBuilder WithServers(IEnumerable<OpenApiOptions.OpenApiServer> servers);
    IOpenApiOptionsBuilder AddServer(string url, string? description = null);
    IOpenApiOptionsBuilder IncludeSecurity(bool includeSecurity);
    OpenApiOptions Build();
}