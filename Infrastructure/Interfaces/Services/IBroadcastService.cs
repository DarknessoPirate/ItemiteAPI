namespace Infrastructure.Interfaces.Services;

public interface IBroadcastService
{
    Task SendMessageToAllUsers(string message);
}