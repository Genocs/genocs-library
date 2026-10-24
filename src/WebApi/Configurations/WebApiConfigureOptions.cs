using Microsoft.Extensions.Options;

namespace Genocs.WebApi.Configurations;

/// <summary>
/// The WebApiOptions definition.
/// </summary>
public class WebApiConfigureOptions : IConfigureNamedOptions<WebApiOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiConfigureOptions"/> class.
    /// </summary>
    public WebApiConfigureOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiConfigureOptions"/> class.
    /// </summary>
    /// <param name="options">The WebApiOptions.</param>
    public WebApiConfigureOptions(IOptions<WebApiOptions> options)
    {
        _ = options;
    }

    public void Configure(string? name, WebApiOptions options)
    {
        Configure(options);
    }

    public void Configure(WebApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Why: keep WebApi option defaults deterministic and avoid mutating values
        // that are already provided by configuration binding.
    }
}