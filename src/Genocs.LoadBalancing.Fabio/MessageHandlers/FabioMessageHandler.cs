using Genocs.LoadBalancing.Fabio.Options;

namespace Genocs.LoadBalancing.Fabio.MessageHandlers;

internal sealed class FabioMessageHandler : DelegatingHandler
{
    private readonly FabioSettings _options;
    private readonly string _servicePath;

    public FabioMessageHandler(FabioSettings options, string? serviceName = null)
    {
        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException("Fabio URL was not provided.");
        }

        _options = options;
        _servicePath = string.IsNullOrWhiteSpace(serviceName) ? string.Empty : $"{serviceName}/";
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return base.SendAsync(request, cancellationToken);
        }

        request.RequestUri = GetRequestUri(request);

        return base.SendAsync(request, cancellationToken);
    }

    private Uri GetRequestUri(HttpRequestMessage request)
        => new($"{_options.Url}/{_servicePath}{request.RequestUri.Host}{request.RequestUri.PathAndQuery}");
}