namespace Genocs.Telemetry.Configurations;

/// <summary>
/// Settings for the Prometheus scraping endpoint exposed on top of the OpenTelemetry MeterProvider.
/// </summary>
/// <remarks>
/// Configured under <c>telemetry.prometheus</c>. When enabled, <c>AddTelemetry()</c> registers
/// the Prometheus exporter on the meter provider and prepares the auth-gating middleware used
/// by <c>UsePrometheus()</c>. The endpoint itself is mapped via <c>MapPrometheus()</c> on the
/// endpoint route builder.
/// </remarks>
public class PrometheusOptions
{
    /// <summary>
    /// Default scraping path used when <see cref="Endpoint"/> is null or whitespace.
    /// </summary>
    public const string DefaultEndpoint = "/metrics";

    /// <summary>
    /// Enables the Prometheus exporter and scraping endpoint.
    /// When disabled, every related helper is a no-op.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The HTTP path the scraping endpoint is mapped to. Defaults to <c>/metrics</c>.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Optional API key required as <c>?apiKey=...</c> query parameter on the scraping endpoint.
    /// Leave null or empty to disable API-key gating.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Optional list of host names (or <c>x-forwarded-for</c> values) allowed to hit the
    /// scraping endpoint. Leave empty to allow any caller (subject to API-key gating, if set).
    /// </summary>
    public IEnumerable<string>? AllowedHosts { get; set; }
}
