namespace Genocs.Messaging;

public interface IBusSubscriber : IDisposable
{
    IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle)
        where T : class;
}