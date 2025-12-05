namespace Domain.DTOs.User;

public class LockUserRequest
{
    public int UserToLockoutId { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string? LockoutMessage { get; set; }
}