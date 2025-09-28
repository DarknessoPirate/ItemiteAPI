using Domain.Entities;

namespace Infrastructure.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}