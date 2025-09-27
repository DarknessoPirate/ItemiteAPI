namespace Domain.Auth;

public class AuthResponse
{
   public int Id { get; set; }
   public required string Username { get; set; }
   public required string Email { get; set; }
   public required string Token { get; set; }
   public string? Location { get; set; } = null;
   public string? PhotoUrl { get; set; } = null;

}