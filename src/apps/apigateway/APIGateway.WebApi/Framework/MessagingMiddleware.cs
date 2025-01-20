using System.Collections.Concurrent;
using Genocs.APIGateway.Configurations;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.MessageBrokers.RabbitMQ.Conventions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Genocs.APIGateway.Framework;

internal class MessagingMiddleware : IMiddleware
{
    private static readonly ConcurrentDictionary<string, IConventions> Conventions = new();
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly RouteMatcher _routeMatcher;
    private readonly ICorrelationContextBuilder _correlationContextBuilder;
    private readonly CorrelationIdFactory _correlationIdFactory;
    private readonly IDictionary<string, List<MessagingOptions.EndpointOptions>> _endpoints;

    public MessagingMiddleware(
                                IRabbitMQClient rabbitMQClient,
                                RouteMatcher routeMatcher,
                                ICorrelationContextBuilder correlationContextBuilder,
                                CorrelationIdFactory correlationIdFactory,
                                IOptions<MessagingOptions> messagingOptions)
    {
        ArgumentNullException.ThrowIfNull(messagingOptions);

        _rabbitMQClient = rabbitMQClient ?? throw new ArgumentNullException(nameof(rabbitMQClient));
        _routeMatcher = routeMatcher ?? throw new ArgumentNullException(nameof(routeMatcher));
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

            string key = $"{endpoint.Exchange}:{endpoint.RoutingKey}";
            if (!Conventions.TryGetValue(key, out var conventions))
            {
                conventions = new MessageConventions(typeof(object), endpoint.RoutingKey, endpoint.Exchange, null);
                Conventions.TryAdd(key, conventions);
            }

            string? spanContext = "TODO: Genocs";
            string messageId = Guid.NewGuid().ToString("N");
            string correlationId = _correlationIdFactory.Create();
            string resourceId = Guid.NewGuid().ToString("N");

            var correlationContext = _correlationContextBuilder.Build(
                                                                        context,
                                                                        correlationId,
                                                                        spanContext,
                                                                        endpoint.RoutingKey,
                                                                        resourceId);

            string content = await new StreamReader(context.Request.Body).ReadToEndAsync();
            object? message = JsonConvert.DeserializeObject(content);

            if (message is not null)
            {
                await _rabbitMQClient.SendAsync(
                                                message,
                                                conventions,
                                                messageId,
                                                correlationId,
                                                spanContext,
                                                correlationContext);
            }

            context.Response.StatusCode = StatusCodes.Status202Accepted;
            return;
        }

        await next(context);
    }
}