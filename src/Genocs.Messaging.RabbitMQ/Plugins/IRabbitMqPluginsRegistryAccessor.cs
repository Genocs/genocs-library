namespace Genocs.Messaging.RabbitMQ.Plugins;

internal interface IRabbitMqPluginsRegistryAccessor
{
    LinkedList<RabbitMqPluginChain> Get();
}