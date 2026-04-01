using Genocs.Http;
using Genocs.Http.Configurations;

namespace Genocs.ServiceDiscovery.Consul.Http;

internal sealed class ConsulHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer, ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
    : GenocsHttpClient(client, options, serializer, correlationContextFactory, correlationIdFactory), IConsulHttpClient;
