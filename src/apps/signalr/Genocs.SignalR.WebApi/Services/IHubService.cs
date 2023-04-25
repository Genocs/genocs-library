using Genocs.SignalR.WebApi.Messages.Events;

namespace Genocs.SignalR.WebApi.Services;

public interface IHubService
{
    Task PublishOperationPendingAsync(OperationPending @event);
    Task PublishOperationCompletedAsync(OperationCompleted @event);
    Task PublishOperationRejectedAsync(OperationRejected @event);
}