namespace Genocs.WebApi.Swagger.Docs.Configurations;

public class SwaggerOptions
{
    public bool Enabled { get; set; }
    public bool ReDocEnabled { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? RoutePrefix { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactUrl { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseUrl { get; set; }
    public string? TermsOfService { get; set; }
    public bool IncludeSecurity { get; set; }
    public bool SerializeAsOpenApiV2 { get; set; }
    public List<OpenApiServer>? Servers { get; set; }

    public class OpenApiServer
    {

        public string? Url { get; set; }
        public string? Description { get; set; }
    }
}

