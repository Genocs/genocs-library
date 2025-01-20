using Genocs.Notifications.WebApi.Framework;
using Genocs.Notifications.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Genocs.Notifications.WebApi.Services;

public class HubWrapper : IHubWrapper
{
    private readonly IHubContext<GenocsHub> _hubContext;

    public HubWrapper(IHubContext<GenocsHub> hubContext)
        => _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

    public async Task PublishToUserAsync(DefaultIdType userId, string message, object data)
        => await _hubContext.Clients.Group(userId.ToUserGroup()).SendAsync(message, data);

    public async Task PublishToAllAsync(string message, object data)
        => await _hubContext.Clients.All.SendAsync(message, data);
}