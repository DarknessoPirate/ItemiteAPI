namespace Domain.DTOs.Email;

public class EmailRequest
{
    public string ToAddress { get; set; }
    public string Subject { get; set; }
}