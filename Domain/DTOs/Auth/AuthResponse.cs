using Domain.DTOs.Auth;
using Domain.Entities;

namespace Domain.Auth;

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public TokenPair Tokens { get; set; }

    public AuthResponse(User user, TokenPair tokens)
    {
        Id = user.Id;
        Email = user.Email!;
        Username = user.UserName!;
        Tokens = tokens;
    }
}