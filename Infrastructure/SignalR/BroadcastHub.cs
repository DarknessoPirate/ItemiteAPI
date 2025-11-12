using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR;

public class BroadcastHub(
    IRequestContextService requestContextService,
    ILogger<BroadcastHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = requestContextService.GetUserIdNullable();

            // anonymous user auditing
            if (userId == null)
            {
                var connectionId = Context.ConnectionId;
                var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                logger.LogInformation(
                    "[BroadcastHub] Anonymous user connected. ConnectionId: {ConnectionId}, IP: {IpAddress}",
                    connectionId, ipAddress);
            }
            else
            {
                // authenticated user auditing
                var username = requestContextService.GetUsername() ?? "null";
                logger.LogDebug(
                    "[BroadcastHub] User: {Username} (ID: {UserId}) connected with connection {ConnectionId}",
                    username, userId, Context.ConnectionId);
            }


            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[BroadcastHub] Error in OnConnectedAsync for connection {Context.ConnectionId}");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = requestContextService.GetUserIdNullable();

            if (userId == null)
            {
                // anonymous user logging
                var connectionId = Context.ConnectionId;
                var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                logger.LogInformation(
                    "[BroadcastHub] Anonymous user disconnected. ConnectionId: {ConnectionId}, IP: {IpAddress}, Reason: {Reason}",
                    connectionId, ipAddress, exception?.Message ?? "Normal disconnect");
            }
            else
            {
                // authenticated user logging
                var username = requestContextService.GetUsername() ?? "null";

                logger.LogDebug(
                    "[BroadcastHub] User: {Username} (ID: {UserId}) disconnected with connection {ConnectionId}. Reason: {Reason}",
                    username, userId.Value, Context.ConnectionId, exception?.Message ?? "Normal disconnect");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[BroadcastHub] Error in OnDisconnectedAsync for connection {ConnectionId}",
                Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}