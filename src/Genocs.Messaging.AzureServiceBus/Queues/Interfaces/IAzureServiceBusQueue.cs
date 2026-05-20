using Genocs.Common.CQRS.Commands;

namespace Genocs.Messaging.AzureServiceBus.Queues.Interfaces;

/// <summary>
/// Azure Service bus.
/// </summary>
public interface IAzureServiceBusQueue
{
    Task SendAsync(ICommand command);
    Task ScheduleAsync(ICommand command, DateTimeOffset offset);

    /// <summary>
    /// Registers a modern command handler contract for queue consumption.
    /// </summary>
    void ConsumeModern<T, TH>()
        where T : class, ICommand
        where TH : ICommandHandler<T>;

    /// <summary>
    /// Registers a legacy command handler contract for queue consumption.
    /// </summary>
    [Obsolete("Consume<T,TH>() uses legacy ICommandHandlerLegacy<T>. Use ConsumeModern<T,TH>() with ICommandHandler<T>. Legacy registration will be removed in a future major release.")]
    void Consume<T, TH>()
        where T : ICommand
        where TH : ICommandHandlerLegacy<T>;
}
