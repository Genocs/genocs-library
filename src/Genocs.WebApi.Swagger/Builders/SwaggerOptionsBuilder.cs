using Genocs.WebApi.Swagger.Docs.Configurations;

namespace Genocs.WebApi.Swagger.Docs.Builders;

internal sealed class SwaggerOptionsBuilder : ISwaggerOptionsBuilder
{
    private readonly SwaggerSettings _options = new();

    public ISwaggerOptionsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public ISwaggerOptionsBuilder ReDocEnable(bool reDocEnabled)
    {
        _options.ReDocEnabled = reDocEnabled;
        return this;
    }

    public ISwaggerOptionsBuilder WithName(string name)
    {
        _options.Name = name;
        return this;
    }

    public ISwaggerOptionsBuilder WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    public ISwaggerOptionsBuilder WithVersion(string version)
    {
        _options.Version = version;
        return this;
    }

    public ISwaggerOptionsBuilder WithRoutePrefix(string routePrefix)
    {
        _options.RoutePrefix = routePrefix;
        return this;
    }

    public ISwaggerOptionsBuilder IncludeSecurity(bool includeSecurity)
    {
        _options.IncludeSecurity = includeSecurity;
        return this;
    }

    public ISwaggerOptionsBuilder SerializeAsOpenApiV2(bool serializeAsOpenApiV2)
    {
        _options.SerializeAsOpenApiV2 = serializeAsOpenApiV2;
        return this;
    }

    public SwaggerSettings Build() => _options;
}