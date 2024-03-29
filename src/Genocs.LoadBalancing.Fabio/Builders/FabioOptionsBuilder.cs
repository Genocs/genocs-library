using Genocs.LoadBalancing.Fabio.Options;

namespace Genocs.LoadBalancing.Fabio.Builders;

/// <summary>
/// The Fabio options builder
/// </summary>
public class FabioOptionsBuilder : IFabioOptionsBuilder
{
    private readonly FabioSettings _options = new();

    /// <summary>
    /// Enable or disable the Fabio load balancer
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns>The option builder to be used for chain the build</returns>
    public IFabioOptionsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    /// <summary>
    /// Set the Fabio url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>The option builder to be used for chain the build</returns>
    public IFabioOptionsBuilder WithUrl(string url)
    {
        _options.Url = url;
        return this;
    }

    /// <summary>
    /// Set the Fabio service name
    /// </summary>
    /// <param name="service">The service name</param>
    /// <returns>The option builder to be used for chain the build</returns>
    public IFabioOptionsBuilder WithService(string service)
    {
        _options.Service = service;
        return this;
    }

    /// <summary>
    /// Build the Fabio options
    /// </summary>
    /// <returns>The Fabio options</returns>
    public FabioSettings Build() => _options;
}