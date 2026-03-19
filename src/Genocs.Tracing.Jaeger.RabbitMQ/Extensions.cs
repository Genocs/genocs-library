using Genocs.Messaging.RabbitMQ;
using Genocs.Tracing.Jaeger.RabbitMQ.Plugins;

namespace Genocs.Tracing.Jaeger.RabbitMQ;

public static class Extensions
{
    public static IRabbitMqPluginsRegistry AddJaegerRabbitMqPlugin(this IRabbitMqPluginsRegistry registry)
    {
        registry.Add<JaegerPlugin>();
        return registry;
    }
}