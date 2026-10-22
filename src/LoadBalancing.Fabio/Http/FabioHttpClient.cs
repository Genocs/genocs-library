using Genocs.Http;
using Genocs.Http.Configurations;

namespace Genocs.LoadBalancing.Fabio.Http;

internal sealed class FabioHttpClient(
    HttpClient client,
    HttpClientOptions options,
    IHttpClientSerializer serializer,
    ICorrelationContextFactory correlationContextFactory,
    ICorrelationIdFactory correlationIdFactory) : GenocsHttpClient(client, options, serializer, correlationContextFactory, correlationIdFactory), IFabioHttpClient;
