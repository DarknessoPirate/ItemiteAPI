using Domain.DTOs.Messages;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR;

[Authorize]
public class NotificationHub(
    IRequestContextService requestContextService,
    ILogger<NotificationHub> logger
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = requestContextService.GetUserIdNullable();
            if (userId == null)
            {
                logger.LogWarning(
                    $"[NotificationHub] Connection {Context.ConnectionId} attempted to connect without an account (no userid found)");
                throw new HubException(
                    "Cannot connect user without a proper id (user needs an account for a messaging feature)");
            }

            var username = requestContextService.GetUsername() ?? "null";
            logger.LogDebug(
                $"[NotificationHub] User: {username} (ID: {userId}) connected with connection {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[NotificationHub] Error in OnConnectedAsync for connection {Context.ConnectionId}");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // at this point the user has to have id (hub refuses connection if no id exception is thrown in OnConnectedAsync)
            var userId = requestContextService.GetUserIdNullable()!.Value;
            var username = requestContextService.GetUsername() ?? "null";

            logger.LogDebug(
                "[NotificationHub] User: {Username} (ID: {UserId}) disconnected with connection {ConnectionId}. Reason: {Reason}",
                username, userId, Context.ConnectionId, exception?.Message ?? "Normal disconnect");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                $"[NotificationHub] Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}