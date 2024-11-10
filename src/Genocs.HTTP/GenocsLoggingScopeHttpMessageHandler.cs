using Genocs.HTTP.Configurations;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Genocs.HTTP;

internal sealed class GenocsLoggingScopeHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly HashSet<string> _maskedRequestUrlParts;
    private readonly string _maskTemplate;

    public GenocsLoggingScopeHttpMessageHandler(ILogger logger, HttpClientOptions settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = settings ?? throw new ArgumentNullException(nameof(settings));
        _maskedRequestUrlParts =
            new HashSet<string>(settings.RequestMasking?.UrlParts ?? Enumerable.Empty<string>());
        _maskTemplate = string.IsNullOrWhiteSpace(settings.RequestMasking?.MaskTemplate)
            ? "*****"
            : settings.RequestMasking.MaskTemplate;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

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

        private static readonly Func<ILogger, HttpMethod, Uri, IDisposable?> _beginRequestPipelineScope =
            LoggerMessage.DefineScope<HttpMethod, Uri>("HTTP {HttpMethod} {Uri}");

        private static readonly Action<ILogger, HttpMethod, Uri, Exception> _requestPipelineStart =
            LoggerMessage.Define<HttpMethod, Uri>(LogLevel.Information, EventIds.PipelineStart, "Start processing HTTP request {HttpMethod} {Uri}");

        private static readonly Action<ILogger, HttpStatusCode, Exception?> _requestPipelineEnd =
            LoggerMessage.Define<HttpStatusCode>(LogLevel.Information, EventIds.PipelineEnd, "End processing HTTP request - {StatusCode}");

        public static IDisposable BeginRequestPipelineScope(
                                                            ILogger logger,
                                                            HttpRequestMessage request,
                                                            ISet<string> maskedRequestUrlParts,
                                                            string maskTemplate)
        {
            var uri = MaskUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
            return _beginRequestPipelineScope(logger, request.Method, uri);
        }

        public static void RequestPipelineStart(
                                                ILogger logger,
                                                HttpRequestMessage request,
                                                ISet<string> maskedRequestUrlParts,
                                                string maskTemplate)
        {
            var uri = MaskUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
            _requestPipelineStart(logger, request.Method, uri, null);
        }

        public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
        {
            _requestPipelineEnd(logger, response.StatusCode, null);
        }

        private static Uri? MaskUri(Uri? uri, ISet<string> maskedRequestUrlParts, string maskTemplate)
        {
            if (!maskedRequestUrlParts.Any())
            {
                return uri;
            }

            string? requestUri = uri?.OriginalString;

            if (string.IsNullOrWhiteSpace(requestUri))
            {
                return uri;
            }

            bool hasMatch = false;
            foreach (string part in maskedRequestUrlParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                if (!requestUri.Contains(part))
                {
                    continue;
                }

                requestUri = requestUri.Replace(part, maskTemplate);
                hasMatch = true;
            }

            return hasMatch ? new Uri(requestUri) : uri;
        }
    }
}