using Infrastructure.Interfaces.Services;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

public class BroadcastService(
    IHubContext<BroadcastHub> broadcastHub
)
    : IBroadcastService
{
    public async Task SendMessageToAllUsers(string message)
    {
        await broadcastHub.Clients
            .All.SendAsync("SiteMessage", message);
    }
}