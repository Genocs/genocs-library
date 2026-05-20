using Genocs.Telemetry.Configurations;
using Microsoft.AspNetCore.Http;

namespace Genocs.Telemetry.Internals;

/// <summary>
/// Auth gate that protects the Prometheus scraping endpoint when an API key
/// or an allowed-host list is configured. When neither is configured, the
/// middleware is a transparent no-op and the request flows through to the
/// mapped scraping endpoint.
/// </summary>
internal sealed class PrometheusMiddleware : IMiddleware
{
    private readonly HashSet<string> _allowedHosts;
    private readonly string _endpoint;
    private readonly string? _apiKey;
    private readonly bool _hasApiKey;
    private readonly bool _hasAllowedHosts;

    public PrometheusMiddleware(PrometheusOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _allowedHosts = new HashSet<string>(options.AllowedHosts ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        _endpoint = OpenTelemetryExtensions.NormalizePrometheusEndpoint(options.Endpoint);
        _apiKey = options.ApiKey;
        _hasApiKey = !string.IsNullOrWhiteSpace(_apiKey);
        _hasAllowedHosts = _allowedHosts.Count > 0;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // Only inspect requests targeting the configured scraping endpoint.
        if (!context.Request.Path.Equals(_endpoint, StringComparison.OrdinalIgnoreCase))
        {
            return next(context);
        }

        // No restrictions configured: let the scraping endpoint handle the request.
        if (!_hasApiKey && !_hasAllowedHosts)
        {
            return next(context);
        }

        if (_hasApiKey
            && context.Request.Query.TryGetValue("apiKey", out var apiKey)
            && string.Equals(apiKey, _apiKey, StringComparison.Ordinal))
        {
            return next(context);
        }

        if (_hasAllowedHosts && IsAllowedHost(context))
        {
            return next(context);
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }

    private bool IsAllowedHost(HttpContext context)
    {
        if (_allowedHosts.Contains(context.Request.Host.Host))
        {
            return true;
        }

        if (context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor)
            && _allowedHosts.Contains(forwardedFor.ToString()))
        {
            return true;
        }

        return false;
    }
}
