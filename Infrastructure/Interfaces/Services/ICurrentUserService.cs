using Domain.DTOs.User;

namespace Infrastructure.Interfaces.Services;

public interface ICurrentUserService
{
    public int GetId();
    public string GetUserName();
    public string GetEmail();
}