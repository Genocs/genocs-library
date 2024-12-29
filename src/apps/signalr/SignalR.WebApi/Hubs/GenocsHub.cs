using Genocs.Auth;
using Genocs.SignalR.WebApi.Framework;
using Microsoft.AspNetCore.SignalR;

namespace Genocs.SignalR.WebApi.Hubs;

public class GenocsHub(IJwtHandler jwtHandler) : Hub
{
    private readonly IJwtHandler _jwtHandler = jwtHandler ?? throw new ArgumentNullException(nameof(jwtHandler));

    public async Task InitializeAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            await DisconnectAsync();
        }

        try
        {
            var payload = _jwtHandler.GetTokenPayload(token);
            if (payload is null || string.IsNullOrEmpty(payload.Subject))
            {
                await DisconnectAsync();
                return;
            }

            string group = Guid.Parse(payload.Subject).ToUserGroup();
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await ConnectAsync();
        }
        catch
        {
            await DisconnectAsync();
        }
    }

    public async Task SendMessage(string user, string message)
        => await Clients.All.SendAsync("ReceiveMessage", user, message);

    private async Task ConnectAsync()
        => await Clients.Client(Context.ConnectionId).SendAsync("connected");

    private async Task DisconnectAsync()
        => await Clients.Client(Context.ConnectionId).SendAsync("disconnected");
}