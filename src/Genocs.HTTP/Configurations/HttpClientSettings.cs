namespace Genocs.HTTP.Configurations;

/// <summary>
/// The HttpClient Settings class.
/// </summary>
public class HttpClientSettings
{
    /// <summary>
    /// It defines if set consul as service discovery or Fabio as load balancer.
    /// Allowed values are: consul, Fabio.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// It defines the number of retries for each request.
    /// </summary>
    public int Retries { get; set; }

    /// <summary>
    /// It defines the list of services to be registered.
    /// </summary>
    public IDictionary<string, string>? Services { get; set; }
    public RequestMaskingSettings? RequestMasking { get; set; }
    public bool RemoveCharsetFromContentType { get; set; }
    public string? CorrelationContextHeader { get; set; }
    public string? CorrelationIdHeader { get; set; }

    public class RequestMaskingSettings
    {
        public bool Enabled { get; set; }
        public IEnumerable<string>? UrlParts { get; set; }
        public string? MaskTemplate { get; set; }
    }
}