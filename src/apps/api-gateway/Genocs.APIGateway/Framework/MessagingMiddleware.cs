using Genocs.APIGateway.Configurations;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.MessageBrokers.RabbitMQ.Conventions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTracing;
using System.Collections.Concurrent;

namespace Genocs.APIGateway.Framework;

internal class MessagingMiddleware : IMiddleware
{
    private static readonly ConcurrentDictionary<string, IConventions> Conventions = new();
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly RouteMatcher _routeMatcher;
    private readonly ITracer _tracer;
    private readonly ICorrelationContextBuilder _correlationContextBuilder;
    private readonly CorrelationIdFactory _correlationIdFactory;
    private readonly IDictionary<string, List<MessagingOptions.EndpointOptions>> _endpoints;

    public MessagingMiddleware(
                                IRabbitMQClient rabbitMQClient,
                                RouteMatcher routeMatcher,
                                ITracer tracer,
                                ICorrelationContextBuilder correlationContextBuilder,
                                CorrelationIdFactory correlationIdFactory,
                                IOptions<MessagingOptions> messagingOptions)
    {
        if (messagingOptions is null)
        {
            throw new ArgumentNullException(nameof(messagingOptions));
        }

        _rabbitMQClient = rabbitMQClient ?? throw new ArgumentNullException(nameof(rabbitMQClient));
        _routeMatcher = routeMatcher ?? throw new ArgumentNullException(nameof(routeMatcher));
        _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        _correlationContextBuilder = correlationContextBuilder ?? throw new ArgumentNullException(nameof(correlationContextBuilder));
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _endpoints = messagingOptions.Value.Endpoints?.Any() is true
            ? messagingOptions.Value.Endpoints.GroupBy(e => e.Method.ToUpperInvariant())
                .ToDictionary(e => e.Key, e => e.ToList())
            : new Dictionary<string, List<MessagingOptions.EndpointOptions>>();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_endpoints.TryGetValue(context.Request.Method, out var endpoints))
        {
            await next(context);
            return;
        }

        foreach (var endpoint in endpoints)
        {
            var match = _routeMatcher.Match(endpoint.Path, context.Request.Path);
            if (match is null)
            {
                continue;
            }

            var key = $"{endpoint.Exchange}:{endpoint.RoutingKey}";
            if (!Conventions.TryGetValue(key, out var conventions))
            {
                conventions = new MessageConventions(typeof(object), endpoint.RoutingKey, endpoint.Exchange, null);
                Conventions.TryAdd(key, conventions);
            }

            var spanContext = _tracer.ActiveSpan is null ? string.Empty : _tracer.ActiveSpan.Context.ToString();
            var messageId = Guid.NewGuid().ToString("N");
            var correlationId = _correlationIdFactory.Create();
            var resourceId = Guid.NewGuid().ToString("N");
            var correlationContext = _correlationContextBuilder.Build(context, correlationId, spanContext,
                endpoint.RoutingKey, resourceId);

            var content = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var message = JsonConvert.DeserializeObject(content);
            _rabbitMQClient.Send(message, conventions, messageId, correlationId, spanContext, correlationContext);
            context.Response.StatusCode = StatusCodes.Status202Accepted;
            return;
        }

        await next(context);
    }
}