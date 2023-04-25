using Genocs.HTTP;
using Genocs.HTTP.Options;

namespace Genocs.LoadBalancing.Fabio.Http;

internal sealed class FabioHttpClient : GenocsHttpClient, IFabioHttpClient
{
    public FabioHttpClient(HttpClient client, HttpClientSettings options, IHttpClientSerializer serializer,
        ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
    {
    }
}