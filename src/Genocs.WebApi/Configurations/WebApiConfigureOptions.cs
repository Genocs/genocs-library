using Microsoft.Extensions.Options;

namespace Genocs.WebApi.Configurations;

/// <summary>
/// The WebApiOptions definition.
/// </summary>
public class WebApiConfigureOptions : IConfigureNamedOptions<WebApiOptions>
{
    private readonly WebApiOptions _options;

    public WebApiConfigureOptions(IOptions<WebApiOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(string? name, WebApiOptions options)
    {
        Configure(options);
    }

    public void Configure(WebApiOptions options)
    {
        throw new NotImplementedException();
    }
}