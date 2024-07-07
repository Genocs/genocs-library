using Genocs.LoadBalancing.Fabio.Configurations;

namespace Genocs.LoadBalancing.Fabio.Builders;

/// <summary>
/// The Fabio options builder.
/// </summary>
public class FabioOptionsBuilder : IFabioOptionsBuilder
{
    private readonly FabioOptions _settings = new();

    /// <summary>
    /// Enable or disable the Fabio load balancer.
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns>The option builder to be used for chain the build.</returns>
    public IFabioOptionsBuilder Enable(bool enabled)
    {
        _settings.Enabled = enabled;
        return this;
    }

    /// <summary>
    /// Set the Fabio url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The option builder to be used for chain the build.</returns>
    public IFabioOptionsBuilder WithUrl(string url)
    {
        _settings.Url = url;
        return this;
    }

    /// <summary>
    /// Set the Fabio service name.
    /// </summary>
    /// <param name="service">The service name</param>
    /// <returns>The option builder to be used for chain the build.</returns>
    public IFabioOptionsBuilder WithService(string service)
    {
        _settings.Service = service;
        return this;
    }

    /// <summary>
    /// Build the Fabio options.
    /// </summary>
    /// <returns>The Fabio options.</returns>
    public FabioOptions Build() => _settings;
}