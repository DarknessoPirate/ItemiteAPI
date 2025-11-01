using Domain.DTOs.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR;

public class NotificationHub(
    ILogger<NotificationHub> logger
    ) : Hub
{

}