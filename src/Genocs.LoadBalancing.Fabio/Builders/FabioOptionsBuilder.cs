using Genocs.LoadBalancing.Fabio.Options;

namespace Genocs.LoadBalancing.Fabio.Builders;

public class FabioOptionsBuilder : IFabioOptionsBuilder
{
    private FabioSettings _options = new();

    public IFabioOptionsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IFabioOptionsBuilder WithUrl(string url)
    {
        _options.Url = url;
        return this;
    }

    public IFabioOptionsBuilder WithService(string service)
    {
        _options.Service = service;
        return this;
    }

    public FabioSettings Build() => _options;
}