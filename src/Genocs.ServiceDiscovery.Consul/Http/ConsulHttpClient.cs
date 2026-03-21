using Genocs.Http;
using Genocs.Http.Configurations;
using Genocs.ServiceDiscovery.Consul;

namespace Genocs.ServiceDiscovery.Consul.Http;

internal sealed class ConsulHttpClient : GenocsHttpClient, IConsulHttpClient
{
    public ConsulHttpClient(
                            HttpClient client,
                            HttpClientOptions options,
                            IHttpClientSerializer serializer,
                            ICorrelationContextFactory correlationContextFactory,
                            ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
    {
    }
}