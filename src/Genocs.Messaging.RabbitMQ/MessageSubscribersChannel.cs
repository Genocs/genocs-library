using Genocs.Messaging.RabbitMQ.Subscribers;
using System.Threading.Channels;

namespace Genocs.Messaging.RabbitMQ;

internal class MessageSubscribersChannel
{
    private readonly Channel<IMessageSubscriber> _channel = Channel.CreateUnbounded<IMessageSubscriber>();

    public ChannelReader<IMessageSubscriber> Reader => _channel.Reader;
    public ChannelWriter<IMessageSubscriber> Writer => _channel.Writer;
}