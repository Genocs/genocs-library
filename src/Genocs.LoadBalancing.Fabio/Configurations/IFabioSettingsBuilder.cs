namespace Genocs.LoadBalancing.Fabio.Configurations;

/// <summary>
/// The Fabio options builder interface definition.
/// </summary>
public interface IFabioSettingsBuilder
{
    /// <summary>
    /// Enable or disable the Fabio load balancer.
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns>The option builder to be used for chain the build.</returns>
    IFabioSettingsBuilder Enable(bool enabled);

    /// <summary>
    /// Set the Fabio url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The option builder to be used for chain the build.</returns>
    IFabioSettingsBuilder WithUrl(string url);

    /// <summary>
    /// Set the Fabio service name
    /// </summary>
    /// <param name="service">The service name.</param>
    /// <returns>The option builder to be used for chain the build</returns>
    IFabioSettingsBuilder WithService(string service);

    /// <summary>
    /// Build the Fabio options
    /// </summary>
    /// <returns>The Fabio options</returns>
    FabioSettings Build();
}