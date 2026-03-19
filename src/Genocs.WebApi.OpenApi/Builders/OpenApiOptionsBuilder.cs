using Genocs.WebApi.OpenApi.Configurations;

namespace Genocs.WebApi.OpenApi.Builders;

internal sealed class OpenApiOptionsBuilder : IOpenApiOptionsBuilder
{
    private readonly OpenApiOptions _options = new();

    public IOpenApiOptionsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IOpenApiOptionsBuilder ReDocEnable(bool reDocEnabled)
    {
        _options.ReDocEnabled = reDocEnabled;
        return this;
    }

    public IOpenApiOptionsBuilder WithName(string name)
    {
        _options.Name = name;
        return this;
    }

    public IOpenApiOptionsBuilder WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    public IOpenApiOptionsBuilder WithVersion(string version)
    {
        _options.Version = version;
        return this;
    }

    public IOpenApiOptionsBuilder WithRoutePrefix(string routePrefix)
    {
        _options.RoutePrefix = routePrefix;
        return this;
    }

    public IOpenApiOptionsBuilder IncludeSecurity(bool includeSecurity)
    {
        _options.IncludeSecurity = includeSecurity;
        return this;
    }

    public IOpenApiOptionsBuilder WithDescription(string description)
    {
        _options.Description = description;
        return this;
    }

    public IOpenApiOptionsBuilder WithContactName(string contactName)
    {
        _options.ContactName = contactName;
        return this;
    }

    public OpenApiOptions Build() => _options;
}