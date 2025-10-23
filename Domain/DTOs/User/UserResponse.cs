using Domain.ValueObjects;

namespace Domain.DTOs.User;

public class UserResponse
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public Location? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
}