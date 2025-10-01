namespace Domain.DTOs.Auth;

public class EmailConfirmationRequest
{
    public string Email { get; set; }
    public string Token { get; set; }
}