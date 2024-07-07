using Genocs.Core.CQRS.Commands;

namespace Genocs.ServiceBusAzure.Queues.Interfaces;

/// <summary>
/// Azure Service bus.
/// </summary>
public interface IAzureServiceBusQueue
{
    Task SendAsync(ICommand command);
    Task ScheduleAsync(ICommand command, DateTimeOffset offset);
    void Consume<T, TH>()
        where T : ICommand
        where TH : ICommandHandlerLegacy<T>;
}
