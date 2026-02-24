using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ.Plugins;

internal interface IRabbitMqPluginAccessor
{
    void SetSuccessor(Func<object, object, BasicDeliverEventArgs, Task> successor);
}