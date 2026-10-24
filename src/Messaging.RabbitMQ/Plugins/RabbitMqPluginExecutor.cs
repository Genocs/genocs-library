using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ.Plugins;

internal sealed class RabbitMqPluginsExecutor(IRabbitMqPluginsRegistryAccessor registry, IServiceProvider serviceProvider) : IRabbitMqPluginsExecutor
{
    private readonly IRabbitMqPluginsRegistryAccessor _registry = registry;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task ExecuteAsync(Func<object, object, BasicDeliverEventArgs, Task> successor, object message, object correlationContext, BasicDeliverEventArgs args)
    {
        var chains = _registry.Get();

        if (chains?.Any() != true)
        {
            await successor(message, correlationContext, args);
            return;
        }

        var plugins = new LinkedList<IRabbitMqPlugin>();

        foreach (var chain in chains)
        {
            object? plugin = _serviceProvider.GetService(chain.PluginType)
                ?? throw new InvalidOperationException($"RabbitMq plugin of type {chain.PluginType.Name} was not registered");

            plugins.AddLast(plugin as IRabbitMqPlugin);
        }

        var current = plugins.Last;

        while (current != null)
        {
            ((IRabbitMqPluginAccessor)current.Value).SetSuccessor(current.Next is null
                ? successor
                : current.Next.Value.HandleAsync);

            current = current.Previous;
        }

        await plugins.First.Value.HandleAsync(message, correlationContext, args);
    }
}