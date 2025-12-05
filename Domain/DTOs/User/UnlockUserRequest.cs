namespace Domain.DTOs.User;

public class UnlockUserRequest
{
    public int UserId { get; set; }
    public string? UnlockMessage { get; set; }
}