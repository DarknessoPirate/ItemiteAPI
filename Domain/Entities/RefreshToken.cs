using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

// token tracking the userid and device id for multiple sessions, supports token chains (refresh tokens replace some tokens and get connected into a chain of tokens)
// if old tokens are reused we can revoke the newest token for security, and force the user to login again 
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;

    public string JwtId { get; set; } =
        string.Empty; // unique jwt id to validate when refreshing (if id of old access token does not match, abort refreshing) 

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    // Important metadata for tokens
    // use it instead of deleting to prevent token reuse, if it is reused, revoke all tokens for this user and log the event
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? ReplacedByTokenId { get; set; }
    public virtual RefreshToken? ReplacedByToken { get; set; }
    public virtual RefreshToken? ReplacedThisToken { get; set; }

    // security
    public string CreatedByIp { get; set; } = string.Empty;
    public string? RevokedByIp { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}