using Microsoft.Extensions.Options;

namespace Genocs.WebApi.Configurations;

/// <summary>
/// The WebApiSettings definition.
/// Move to WebApiSettings.cs.
/// </summary>
public class WebApiOptions : IConfigureNamedOptions<WebApiSettings>
{
    private readonly WebApiSettings _jwtSettings;

    public WebApiOptions(IOptions<WebApiSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public void Configure(string? name, WebApiSettings options)
    {
        Configure(options);
    }

    public void Configure(WebApiSettings options)
    {
        throw new NotImplementedException();
    }
}