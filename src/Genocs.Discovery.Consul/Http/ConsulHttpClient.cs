using Genocs.HTTP;
using Genocs.HTTP.Configurations;

namespace Genocs.Discovery.Consul.Http;

internal sealed class ConsulHttpClient : GenocsHttpClient, IConsulHttpClient
{
    public ConsulHttpClient(HttpClient client, HttpClientSettings options, IHttpClientSerializer serializer,
        ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
    {
    }
}