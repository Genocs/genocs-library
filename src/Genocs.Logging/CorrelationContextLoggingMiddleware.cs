using System.Diagnostics;
using System.Text;
using Genocs.Logging.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Logging;

public class CorrelationContextLoggingMiddleware : IMiddleware
{
    private readonly ILogger<CorrelationContextLoggingMiddleware> _logger;
    private readonly HttpPayloadOptions _payloadOptions;

    public CorrelationContextLoggingMiddleware(
        ILogger<CorrelationContextLoggingMiddleware> logger,
        LoggerOptions loggerOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _payloadOptions = loggerOptions?.HttpPayload ?? new HttpPayloadOptions();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var scopeData = new Dictionary<string, object>();

        if (Activity.Current is { } activity)
        {
            foreach ((string key, string? value) in activity.Baggage)
            {
                if (!string.IsNullOrWhiteSpace(key) && value is not null)
                {
                    scopeData[key] = value;
                }
            }
        }

        if (_payloadOptions.Enabled && _payloadOptions.CaptureRequestBody)
        {
            string? requestBody = await ReadRequestBodyAsync(context.Request);
            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                scopeData["HttpRequestBody"] = requestBody;
                Activity.Current?.SetTag("http.request.body", requestBody);
            }
        }

        using (_logger.BeginScope(scopeData))
        {
            if (!_payloadOptions.Enabled || !_payloadOptions.CaptureResponseBody)
            {
                await next(context);
                return;
            }

            var originalResponseBody = context.Response.Body;
            await using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            try
            {
                await next(context);

                string? responseBody = await ReadResponseBodyAsync(context.Response, responseBuffer);
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    scopeData["HttpResponseBody"] = responseBody;
                    Activity.Current?.SetTag("http.response.body", responseBody);
                }
            }
            finally
            {
                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanRead || !IsCaptureCandidate(request.ContentType))
        {
            return null;
        }

        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        string payload = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return Truncate(payload);
    }

    private async Task<string?> ReadResponseBodyAsync(HttpResponse response, MemoryStream buffer)
    {
        if (!IsCaptureCandidate(response.ContentType))
        {
            return null;
        }

        buffer.Position = 0;
        using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        string payload = await reader.ReadToEndAsync();
        return Truncate(payload);
    }

    private bool IsCaptureCandidate(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        string normalized = contentType.ToLowerInvariant();

        foreach (string allowed in _payloadOptions.AllowedContentTypes)
        {
            if (string.IsNullOrWhiteSpace(allowed))
            {
                continue;
            }

            string pattern = allowed.ToLowerInvariant();

            if (pattern.EndsWith("/*", StringComparison.Ordinal))
            {
                string prefix = pattern[..^1];
                if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            if (pattern.Contains("*+json", StringComparison.Ordinal) && normalized.EndsWith("+json", StringComparison.Ordinal))
            {
                return true;
            }

            if (normalized.Contains(pattern, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private string Truncate(string payload)
    {
        if (string.IsNullOrEmpty(payload) || _payloadOptions.MaxBodyLength <= 0)
        {
            return string.Empty;
        }

        if (payload.Length <= _payloadOptions.MaxBodyLength)
        {
            return payload;
        }

        return payload[.._payloadOptions.MaxBodyLength];
    }
}