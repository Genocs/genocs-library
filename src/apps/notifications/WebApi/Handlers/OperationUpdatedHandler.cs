using Genocs.Common.Cqrs.Events;
using Genocs.Notifications.WebApi.Messages.Events;
using Genocs.Notifications.WebApi.Services;

namespace Genocs.Notifications.WebApi.Handlers;

public class OperationUpdatedHandler : IEventHandler<OperationPending>,
    IEventHandler<OperationCompleted>, IEventHandler<OperationRejected>
{
    private readonly IHubService _hubService;

    public OperationUpdatedHandler(IHubService hubService)
        => _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));

    public async Task HandleAsync(OperationPending @event, CancellationToken cancellationToken = default)
        => await _hubService.PublishOperationPendingAsync(@event);

    public async Task HandleAsync(OperationCompleted @event, CancellationToken cancellationToken = default)
        => await _hubService.PublishOperationCompletedAsync(@event);

    public async Task HandleAsync(OperationRejected @event, CancellationToken cancellationToken = default)
        => await _hubService.PublishOperationRejectedAsync(@event);

}