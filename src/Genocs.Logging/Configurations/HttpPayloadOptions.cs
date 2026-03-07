namespace Genocs.Logging.Configurations;

/// <summary>
/// Controls optional HTTP payload capture for request-scoped logs and activity tags.
/// </summary>
public class HttpPayloadOptions
{
    /// <summary>
    /// Enables payload capture.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Captures the request body when enabled.
    /// </summary>
    public bool CaptureRequestBody { get; set; }

    /// <summary>
    /// Captures the response body when enabled.
    /// </summary>
    public bool CaptureResponseBody { get; set; } = true;

    /// <summary>
    /// Maximum body size captured in characters.
    /// </summary>
    public int MaxBodyLength { get; set; } = 4096;

    /// <summary>
    /// Content types allowed for payload capture.
    /// </summary>
    public ICollection<string> AllowedContentTypes { get; set; } = new[]
    {
        "application/json",
        "application/xml",
        "text/",
        "application/*+json"
    };
}