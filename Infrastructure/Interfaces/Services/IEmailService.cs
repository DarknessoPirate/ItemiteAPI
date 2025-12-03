using Domain.DTOs.Email;
using Domain.Entities;

namespace Infrastructure.Interfaces.Services;

public interface IEmailService
{
    Task SendConfirmationAsync(User user, string emailToken);
    Task SendPasswordResetTokenAsync(User user, string passwordResetToken);
    Task SendEmailChangeTokenAsync(User user,string newEmail, string emailToken);
    Task SendGlobalNotificationAsync(List<User> recipients, string emailSubject, string title, string message);
    Task SendNotificationAsync(User recipient, string emailSubject, string title, string message);
}