using Microsoft.Extensions.Options;

namespace Genocs.WebApi.Configurations;

/// <summary>
/// The WebApiOptions definition.
/// </summary>
public class WebApiConfigureOptions(IOptions<WebApiOptions> options) : IConfigureNamedOptions<WebApiOptions>
{
    public void Configure(string? name, WebApiOptions options)
    {
        Configure(options);
    }

    public void Configure(WebApiOptions options)
    {
        throw new NotImplementedException();
    }
}