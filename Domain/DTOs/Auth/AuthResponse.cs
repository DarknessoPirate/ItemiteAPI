using Domain.Entities;

namespace Domain.Auth;

public class AuthResponse
{
   public int Id { get; set; }
   public string Username { get; set; }
   public string Email { get; set; }
   public string Token { get; set; }
   public DateTime TokenExpirationDate { get; set; }
   public string RefreshToken { get; set; }
   public DateTime RefreshTokenExpirationDate { get; set; }
   
   public AuthResponse(User user, TokenResponse accessToken, TokenResponse refreshToken)
   {
      Id = user.Id;
      Email = user.Email!;
      Username = user.UserName!;
      Token = accessToken.Token;
      TokenExpirationDate = accessToken.TokenExpirationDate;
      RefreshToken = refreshToken.Token;
      RefreshTokenExpirationDate = refreshToken.TokenExpirationDate;
   }
}