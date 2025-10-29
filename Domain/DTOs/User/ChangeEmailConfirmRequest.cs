namespace Domain.DTOs.User;

public class ChangeEmailConfirmRequest
{
    public string CurrentEmail { get; set; }
    public string Token { get; set; }
}