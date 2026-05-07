using System.Reflection;

namespace Genocs.Messaging.RabbitMQ.Conventions;

public class ConventionsBuilder : IConventionsBuilder
{
    private readonly RabbitMQOptions _options;
    private readonly bool _snakeCase;
    private readonly string _queueTemplate;

    public ConventionsBuilder(RabbitMQOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _queueTemplate = string.IsNullOrWhiteSpace(_options.Queue?.Template)
            ? "{{assembly}}/{{exchange}}.{{message}}"
            : _options.Queue.Template;

        _snakeCase = _options.ConventionsCasing?.Equals("snakeCase", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    public string? GetRoutingKey(Type type)
    {
        string routingKey = type.Name;
        if (_options.Conventions?.MessageAttribute?.IgnoreRoutingKey is true)
        {
            return WithCasing(routingKey);
        }

        var attribute = GeAttribute(type);
        routingKey = string.IsNullOrWhiteSpace(attribute?.RoutingKey) ? routingKey : attribute.RoutingKey;

        return WithCasing(routingKey);
    }

    public string? GetExchange(Type type)
    {
        string? exchange = string.IsNullOrWhiteSpace(_options.Exchange?.Name)
            ? type.Assembly.GetName().Name
            : _options.Exchange.Name;

        if (_options.Conventions?.MessageAttribute?.IgnoreExchange is true)
        {
            return WithCasing(exchange);
        }

        var attribute = GeAttribute(type);
        exchange = string.IsNullOrWhiteSpace(attribute?.Exchange) ? exchange : attribute.Exchange;

        return WithCasing(exchange);
    }

    public string? GetQueue(Type type)
    {
        var attribute = GeAttribute(type);
        bool? ignoreQueue = _options.Conventions?.MessageAttribute?.IgnoreQueue;
        if (ignoreQueue is null or false && !string.IsNullOrWhiteSpace(attribute?.Queue))
        {
            return WithCasing(attribute.Queue);
        }

        bool? ignoreExchange = _options.Conventions?.MessageAttribute?.IgnoreExchange;
        string? assembly = type.Assembly.GetName().Name;
        string? message = type.Name;

        string? exchange = ignoreExchange is true
            ? _options.Exchange?.Name
            : string.IsNullOrWhiteSpace(attribute?.Exchange)
                ? _options.Exchange?.Name
                : attribute.Exchange;

        string? queue = _queueTemplate.Replace("{{assembly}}", assembly)
            .Replace("{{exchange}}", exchange)
            .Replace("{{message}}", message);

        return WithCasing(queue);
    }

    private string? WithCasing(string? value) => _snakeCase ? SnakeCase(value) : value;

    private static string? SnakeCase(string? value)
        => string.Concat((value ?? string.Empty).Select((x, i) =>
                i > 0 && value![i - 1] != '.' && value![i - 1] != '/' && char.IsUpper(x) ? "_" + x : x.ToString()))
            .ToLowerInvariant();

    private static MessageAttribute? GeAttribute(MemberInfo type)
        => type.GetCustomAttribute<MessageAttribute>();
}