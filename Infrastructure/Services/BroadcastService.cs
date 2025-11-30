using Domain.DTOs.Notifications;
using Infrastructure.Interfaces.Services;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

public class BroadcastService(
    IHubContext<BroadcastHub> broadcastHub
)
    : IBroadcastService
{
    public async Task SendMessageToAllUsers(NotificationInfo notificationInfo)
    {
        await broadcastHub.Clients
            .All.SendAsync("SiteMessage", notificationInfo);
    }
}