namespace Domain.Enums;

public enum GoogleLoginResult
{
    Success,
    NoClaimsFailure,
    NoEmailFailure,
    UsernameUniqueFailure,
    EmailUniqueFailure,
    AccountLocked
}