
namespace Genocs.WebApi.Swagger.Configurations;

public class OpenApiSettings
{
    public static string Position = "OpenApi";

    /// <summary>
    /// Gets or sets a value indicating whether the Swagger is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ReDoc is enabled.
    /// </summary>
    public bool ReDocEnabled { get; set; }

    /// <summary>
    /// The Swagger documentation title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The Swagger documentation version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The Swagger documentation description.
    /// </summary>
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactUrl { get; set; }
    public bool License { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseUrl { get; set; }
    public string? TermsAndConditionsUrl { get; set; }
    public bool SerializeAsOpenApiV2 { get; set; }
    public string? RoutePrefix { get; set; }
    public bool IncludeSecurity { get; set; }
    public IEnumerable<ServerDetails>? Servers { get; internal set; }
}

public class ServerDetails
{
    public string? Url { get; set; }
    public string? Description { get; set; }
}