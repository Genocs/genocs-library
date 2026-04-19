using Genocs.Http.Configurations;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Genocs.Http;

internal sealed class GenocsLoggingScopeHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly HashSet<string> _maskedRequestUrlParts;
    private readonly string _maskTemplate;

    public GenocsLoggingScopeHttpMessageHandler(ILogger logger, HttpClientOptions settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = settings ?? throw new ArgumentNullException(nameof(settings));

        _maskedRequestUrlParts = [.. settings.RequestMasking?.UrlParts ?? []];

        _maskTemplate = string.IsNullOrWhiteSpace(settings.RequestMasking?.MaskTemplate)
            ? "*****"
            : settings.RequestMasking.MaskTemplate;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using (Log.BeginRequestPipelineScope(_logger, request, _maskedRequestUrlParts, _maskTemplate))
        {
            Log.RequestPipelineStart(_logger, request, _maskedRequestUrlParts, _maskTemplate);
            var response = await base.SendAsync(request, cancellationToken);
            Log.RequestPipelineEnd(_logger, response);

            return response;
        }
    }

    private static class Log
    {
        private static class EventIds
        {
            public static readonly EventId PipelineStart = new(100, "RequestPipelineStart");
            public static readonly EventId PipelineEnd = new(101, "RequestPipelineEnd");
        }

        private static readonly Func<ILogger, HttpMethod, string, IDisposable?> _beginRequestPipelineScope =
            LoggerMessage.DefineScope<HttpMethod, string>("Http {HttpMethod} {Uri}");

        private static readonly Action<ILogger, HttpMethod, string, Exception?> _requestPipelineStart =
            LoggerMessage.Define<HttpMethod, string>(LogLevel.Information, EventIds.PipelineStart, "Start processing Http request {HttpMethod} {Uri}");

        private static readonly Action<ILogger, HttpStatusCode, Exception?> _requestPipelineEnd =
            LoggerMessage.Define<HttpStatusCode>(LogLevel.Information, EventIds.PipelineEnd, "End processing Http request - {StatusCode}");

        public static IDisposable BeginRequestPipelineScope(
                                                            ILogger logger,
                                                            HttpRequestMessage request,
                                                            ISet<string> maskedRequestUrlParts,
                                                            string maskTemplate)
        {
            var uri = BuildLogUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
            return _beginRequestPipelineScope(logger, request.Method, uri) ?? NoopDisposableScope.Instance;
        }

        public static void RequestPipelineStart(
                                                ILogger logger,
                                                HttpRequestMessage request,
                                                ISet<string> maskedRequestUrlParts,
                                                string maskTemplate)
        {
            var uri = BuildLogUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
            _requestPipelineStart(logger, request.Method, uri, null);
        }

        public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
        {
            _requestPipelineEnd(logger, response.StatusCode, null);
        }

        private static string BuildLogUri(Uri? uri, ISet<string> maskedRequestUrlParts, string maskTemplate)
        {
            string requestUri = uri?.OriginalString ?? string.Empty;
            if (string.IsNullOrEmpty(requestUri) || !maskedRequestUrlParts.Any())
            {
                return requestUri;
            }

            foreach (string part in maskedRequestUrlParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                // Request masking is exact-token replacement for URL rendering only.
                requestUri = requestUri.Replace(part, maskTemplate, StringComparison.Ordinal);
            }

            return requestUri;
        }

        private sealed class NoopDisposableScope : IDisposable
        {
            public static readonly NoopDisposableScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}