using System.Collections.Concurrent;

namespace Genocs.Messaging.RabbitMQ.Conventions;

public class ConventionsProvider(IConventionsRegistry registry, IConventionsBuilder builder) : IConventionsProvider
{
    private readonly ConcurrentDictionary<Type, IConventions> _conventions = new();

    private readonly IConventionsRegistry _registry = registry;
    private readonly IConventionsBuilder _builder = builder;

    public IConventions Get<T>() => Get(typeof(T));

    public IConventions Get(Type type)
    {
        if (_conventions.TryGetValue(type, out var conventions))
        {
            return conventions;
        }

        conventions = _registry.Get(type) ?? new MessageConventions(type, _builder.GetRoutingKey(type), _builder.GetExchange(type), _builder.GetQueue(type));

        _conventions.TryAdd(type, conventions);

        return conventions;
    }
}