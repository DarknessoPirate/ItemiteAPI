using Domain.DTOs.Email;
using Domain.Entities;

namespace Infrastructure.Interfaces.Services;

public interface IEmailService
{
    Task SendConfirmationAsync(EmailRequest emailRequest, string username, string confirmationLink);
}