namespace Domain.DTOs.Email;

public class EmailChangeConfirmationModel
{
    public string UserName { get; set; } = string.Empty;
    public string ConfirmationLink { get; set; } = string.Empty;
    public string? NewEmail { get; set; }
}