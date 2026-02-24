namespace Genocs.WebApi.OpenApi.Docs.Configurations;

public class SwaggerOptions
{
    /// <summary>
    /// The flag to enable or disable Swagger.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The flag to enable or disable ReDoc.
    /// </summary>
    public bool ReDocEnabled { get; set; }

    /// <summary>
    /// The name of the API.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The title of the API. You can use this field to set the title by using markdown.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The version of the API.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The description of the API. You can use this field to set the description by using markdown.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The route prefix of the API.
    /// </summary>
    public string? RoutePrefix { get; set; }

    /// <summary>
    /// The contact name of the API.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// The contact email of the API.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// The contact URL of the API.
    /// </summary>
    public string? ContactUrl { get; set; }

    /// <summary>
    /// The license name of the API.
    /// </summary>
    public string? LicenseName { get; set; }

    /// <summary>
    /// The license URL of the API.
    /// </summary>
    public string? LicenseUrl { get; set; }

    /// <summary>
    /// The terms of service of the API. you can use this field to set the terms of service by using markdown or as url link.
    /// </summary>
    public string? TermsOfService { get; set; }

    /// <summary>
    /// The flag to include security. By using this flag, you can include the security information in the Swagger document.
    /// </summary>
    public bool IncludeSecurity { get; set; }

    /// <summary>
    /// List of servers that support this API.
    /// </summary>
    public List<OpenApiServer>? Servers { get; set; }

    /// <summary>
    /// Internal class to represent the server information in the Swagger document.
    /// </summary>
    public class OpenApiServer
    {

        /// <summary>
        /// The URL of the server.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// The description of the server.
        /// </summary>
        public string? Description { get; set; }
    }
}