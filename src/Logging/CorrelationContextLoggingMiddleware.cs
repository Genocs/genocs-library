using System.Diagnostics;
using System.Text;
using Genocs.Logging.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Genocs.Logging;

public class CorrelationContextLoggingMiddleware : IMiddleware
{
    private const int DefaultMaxBodyLength = 4096;
    private const int MaxSupportedBodyLength = 16384;
    private const int MaxBaggageItems = 32;
    private const int MaxBaggageKeyLength = 64;
    private const int MaxBaggageValueLength = 256;
    private static readonly string[] DefaultAllowedContentTypes =
    [
        "application/json",
        "application/*+json"
    ];

    private readonly ILogger<CorrelationContextLoggingMiddleware> _logger;
    private readonly HttpPayloadOptions _payloadOptions;

    public CorrelationContextLoggingMiddleware(ILogger<CorrelationContextLoggingMiddleware> logger, LoggerOptions loggerOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _payloadOptions = NormalizePayloadOptions(loggerOptions?.HttpPayload ?? new HttpPayloadOptions());
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var scopeData = new Dictionary<string, object>();

        if (Activity.Current is { } activity)
        {
            AddBoundedBaggage(scopeData, activity);
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
                    // Response payload is only available after the request pipeline completes,
                    // so enrich the activity instead of mutating the request scope state.
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
        string? mediaType = NormalizeContentType(contentType);
        if (string.IsNullOrWhiteSpace(mediaType))
        {
            return false;
        }

        foreach (string allowed in _payloadOptions.AllowedContentTypes)
        {
            string? pattern = NormalizeContentType(allowed);
            if (string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            if (pattern.Equals("*/*", StringComparison.Ordinal))
            {
                return true;
            }

            if (pattern.EndsWith("/*", StringComparison.Ordinal))
            {
                int slashIndex = pattern.IndexOf('/', StringComparison.Ordinal);
                if (slashIndex < 0)
                {
                    continue;
                }

                string prefix = pattern[..(slashIndex + 1)];
                if (mediaType.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }

                continue;
            }

            if (pattern.Contains("*+", StringComparison.Ordinal))
            {
                int wildcardIndex = pattern.IndexOf('*', StringComparison.Ordinal);
                int plusIndex = pattern.IndexOf('+', StringComparison.Ordinal);
                if (wildcardIndex < 0 || plusIndex < 0 || plusIndex <= wildcardIndex)
                {
                    continue;
                }

                string prefix = pattern[..wildcardIndex];
                string suffix = pattern[plusIndex..];
                if (mediaType.StartsWith(prefix, StringComparison.Ordinal)
                    && mediaType.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return true;
                }

                continue;
            }

            if (string.Equals(mediaType, pattern, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private string Truncate(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return string.Empty;
        }

        if (payload.Length <= _payloadOptions.MaxBodyLength)
        {
            return payload;
        }

        return payload[.._payloadOptions.MaxBodyLength];
    }

    private static HttpPayloadOptions NormalizePayloadOptions(HttpPayloadOptions source)
    {
        int maxBodyLength = source.MaxBodyLength <= 0
            ? DefaultMaxBodyLength
            : Math.Min(source.MaxBodyLength, MaxSupportedBodyLength);

        string[] allowedContentTypes = source.AllowedContentTypes
            .Select(NormalizeContentType)
            .Where(static contentType => !string.IsNullOrWhiteSpace(contentType))
            .Select(static contentType => contentType!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new HttpPayloadOptions
        {
            Enabled = source.Enabled,
            CaptureRequestBody = source.CaptureRequestBody,
            CaptureResponseBody = source.CaptureResponseBody,
            MaxBodyLength = maxBodyLength,
            AllowedContentTypes = allowedContentTypes.Length == 0
                ? [.. DefaultAllowedContentTypes]
                : allowedContentTypes
        };
    }

    private static string? NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        string candidate = contentType;
        int separatorIndex = candidate.IndexOf(';', StringComparison.Ordinal);
        if (separatorIndex >= 0)
        {
            candidate = candidate[..separatorIndex];
        }

        candidate = candidate.Trim().ToLowerInvariant();
        return candidate.Contains('/', StringComparison.Ordinal) ? candidate : null;
    }

    private static void AddBoundedBaggage(IDictionary<string, object> scopeData, Activity activity)
    {
        int processedItems = 0;

        foreach ((string key, string? value) in activity.Baggage)
        {
            if (processedItems >= MaxBaggageItems)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(key) || value is null)
            {
                continue;
            }

            string normalizedKey = TruncateToLength(key.Trim(), MaxBaggageKeyLength);
            if (string.IsNullOrWhiteSpace(normalizedKey) || scopeData.ContainsKey(normalizedKey))
            {
                continue;
            }

            scopeData[normalizedKey] = TruncateToLength(value, MaxBaggageValueLength);
            processedItems++;
        }
    }

    private static string TruncateToLength(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input.Length <= maxLength ? input : input[..maxLength];
    }
}