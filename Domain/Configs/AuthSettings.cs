namespace Domain.Configs;

public class AuthSettings
{
    public bool IsEmailConfirmationRequired { get; set; } = false;
    public int EmailTokenLifespanInMinutes { get; set; } = 1440;
    public string EmailVerificationUri {get; set;} = "http://localhost:4200/confirm-email";
}