namespace Genocs.Messaging.RabbitMQ.Subscribers;

internal sealed class RabbitMQSubscriber : IBusSubscriber
{
    private readonly MessageSubscribersChannel _messageSubscribersChannel;

    public RabbitMQSubscriber(MessageSubscribersChannel messageSubscribersChannel)
    {
        _messageSubscribersChannel = messageSubscribersChannel;
    }

    public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle)
        where T : class
    {
        var type = typeof(T);

        _messageSubscribersChannel.Writer.TryWrite(MessageSubscriber.Subscribe(type, (serviceProvider, message, context)
            => handle(serviceProvider, (T)message, context)));

        return this;
    }

    public void Dispose()
    {
    }
}