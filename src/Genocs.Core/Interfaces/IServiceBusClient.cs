using System.Threading.Tasks;

namespace Genocs.Core.Interfaces
{
    /// <summary>
    /// The generic BusClient interface
    /// </summary>
    public interface IServiceBusClient
    {
        Task SendCommandAsync<T>(T @command) where T : ICommand;
        Task PublishEventAsync<T>(T @event) where T : IEvent;
    }
}
