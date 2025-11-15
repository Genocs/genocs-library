using Genocs.Notifications.WebApi.Messages.Events;

namespace Genocs.Notifications.WebApi.Services;

public interface IHubService
{
    Task PublishOperationPendingAsync(OperationPending @event);
    Task PublishOperationCompletedAsync(OperationCompleted @event);
    Task PublishOperationRejectedAsync(OperationRejected @event);
}