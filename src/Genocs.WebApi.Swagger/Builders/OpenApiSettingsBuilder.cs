using Genocs.WebApi.Swagger.Configurations;

namespace Genocs.WebApi.Swagger.Builders;

internal sealed class OpenApiSettingsBuilder : IOpenApiSettingsBuilder
{
    private readonly OpenApiSettings _options = new();

    public IOpenApiSettingsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IOpenApiSettingsBuilder ReDocEnable(bool reDocEnabled)
    {
        _options.ReDocEnabled = reDocEnabled;
        return this;
    }

    public IOpenApiSettingsBuilder WithName(string name)
    {
        _options.Name = name;
        return this;
    }

    public IOpenApiSettingsBuilder WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    public IOpenApiSettingsBuilder WithVersion(string version)
    {
        _options.Version = version;
        return this;
    }

    public IOpenApiSettingsBuilder WithRoutePrefix(string routePrefix)
    {
        _options.RoutePrefix = routePrefix;
        return this;
    }

    public IOpenApiSettingsBuilder IncludeSecurity(bool includeSecurity)
    {
        _options.IncludeSecurity = includeSecurity;
        return this;
    }

    public IOpenApiSettingsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2)
    {
        _options.SerializeAsOpenApiV2 = serializeAsOpenApiV2;
        return this;
    }

    public OpenApiSettings Build()
        => _options;
}