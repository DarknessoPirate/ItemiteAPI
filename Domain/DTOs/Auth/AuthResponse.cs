using Domain.Entities;

namespace Domain.Auth;

public class AuthResponse
{
   public int Id { get; set; }
   public string Username { get; set; }
   public string Email { get; set; }
   public TokenResponse AccessToken { get; set; }
   public TokenResponse RefreshToken { get; set; }
   
   public AuthResponse(User user, TokenResponse accessToken, TokenResponse refreshToken)
   {
      Id = user.Id;
      Email = user.Email!;
      Username = user.UserName!;
      AccessToken = accessToken;
      RefreshToken = refreshToken;
   }
}