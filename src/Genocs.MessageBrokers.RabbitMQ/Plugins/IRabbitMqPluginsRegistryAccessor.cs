namespace Genocs.MessageBrokers.RabbitMQ.Plugins;

internal interface IRabbitMqPluginsRegistryAccessor
{
    LinkedList<RabbitMqPluginChain> Get();
}