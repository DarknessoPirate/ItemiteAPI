namespace Domain.DTOs.Email;

public class EmailConfirmationModel
{
    public string UserName { get; set; } = string.Empty;
    public string ConfirmationLink { get; set; } = string.Empty;
}