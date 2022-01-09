using System.Threading.Tasks;

namespace Genocs.Core.Interfaces
{
    /// <summary>
    /// Event definition
    /// </summary>
    public interface IEvent : IMessage
    {
    }
    public interface IEventHandler<T> where T : IEvent
    {
        Task HandleEvent(T @event);
    }
}
