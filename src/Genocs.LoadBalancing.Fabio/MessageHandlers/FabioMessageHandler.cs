using Genocs.LoadBalancing.Fabio.Configurations;

namespace Genocs.LoadBalancing.Fabio.MessageHandlers;

internal sealed class FabioMessageHandler : DelegatingHandler
{
    private readonly FabioSettings _settings;
    private readonly string _servicePath;

    public FabioMessageHandler(FabioSettings settings, string? serviceName = null)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            throw new InvalidOperationException("Fabio URL was not provided.");
        }

        _settings = settings;
        _servicePath = string.IsNullOrWhiteSpace(serviceName) ? string.Empty : $"{serviceName}/";
    }

    protected override Task<HttpResponseMessage> SendAsync(
                                                            HttpRequestMessage request,
                                                            CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return base.SendAsync(request, cancellationToken);
        }

        request.RequestUri = GetRequestUri(request);

        return base.SendAsync(request, cancellationToken);
    }

    private Uri GetRequestUri(HttpRequestMessage request)
        => new($"{_settings.Url}/{_servicePath}{request.RequestUri.Host}{request.RequestUri.PathAndQuery}");
}