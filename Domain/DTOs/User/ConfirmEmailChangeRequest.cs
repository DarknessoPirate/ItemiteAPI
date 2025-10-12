namespace Domain.DTOs.User;

public class ConfirmEmailChangeRequest
{
    public string CurrentEmail { get; set; }
    public string Token { get; set; }
}