using Genocs.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genocs.ServiceBusAzure.Queues.Interfaces
{
    public interface IAzureServiceBusQueue
    {
        Task SendAsync(ICommand command);
        Task ScheduleAsync(ICommand command, DateTimeOffset offset);
        void Consume<T, TH>() where T : ICommand where TH : ICommandHandler<T>;
    }
}
