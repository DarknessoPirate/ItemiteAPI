using Domain.ValueObjects;

namespace Infrastructure.Configuration.Seeding;

public class InitialUser
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Location? Location { get; set; }
    //public string? PhotoUrl { get; set; }
}