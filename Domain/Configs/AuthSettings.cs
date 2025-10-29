namespace Domain.Configs;

public class AuthSettings
{
    public bool IsEmailConfirmationRequired { get; set; }
    public int EmailTokenLifespanInMinutes { get; set; }
    public string EmailVerificationUri { get; set; }
    public string EmailChangeConfirmationUri  { get; set; }
    public string PasswordResetUri { get; set; }
}