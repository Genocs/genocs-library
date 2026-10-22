using Genocs.Http.Configurations;
using System.Net.Http.Headers;

namespace Genocs.Http;

internal sealed class GenocsCorrelationHeadersHttpMessageHandler : DelegatingHandler
{
    private readonly HttpClientOptions _settings;
    private readonly ICorrelationContextFactory _correlationContextFactory;
    private readonly ICorrelationIdFactory _correlationIdFactory;

    public GenocsCorrelationHeadersHttpMessageHandler(
        HttpClientOptions settings,
        ICorrelationContextFactory correlationContextFactory,
        ICorrelationIdFactory correlationIdFactory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        AddHeaderIfMissing(request.Headers, _settings.CorrelationContextHeader, _correlationContextFactory.Create());
        AddHeaderIfMissing(request.Headers, _settings.CorrelationIdHeader, _correlationIdFactory.Create());

        return base.SendAsync(request, cancellationToken);
    }

    private static void AddHeaderIfMissing(HttpRequestHeaders headers, string headerName, string? headerValue)
    {
        if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
        {
            return;
        }

        if (headers.Contains(headerName))
        {
            return;
        }

        headers.TryAddWithoutValidation(headerName, headerValue);
    }
}