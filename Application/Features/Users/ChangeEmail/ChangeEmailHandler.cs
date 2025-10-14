using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Users.ChangeEmail;

// use this to send the email change token to the new email. Click the link to finally change the email and confirm it is a valid email.
public class ChangeEmailHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IOptions<AuthSettings> authSettings,
    ILogger<ChangeEmailHandler> logger
) : IRequestHandler<ChangeEmailCommand>
{
    public async Task Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await userManager.FindByIdAsync(request.UserId.ToString());
        if (currentUser == null)
            throw new BadRequestException("User not found");

        var userExists = await userManager.FindByEmailAsync(request.changeEmailRequest.NewEmail);
        if (userExists != null && userExists.Id != currentUser.Id)
            throw new BadRequestException("Email is already taken");

        var correctPassword = await userManager.CheckPasswordAsync(currentUser, request.changeEmailRequest.Password);
        if (!correctPassword)
            throw new UnauthorizedException("Incorrect password");
        
        if(currentUser.Email == request.changeEmailRequest.NewEmail)
            throw new BadRequestException("You cannot change the email to the same one");

        var changeEmailToken = await userManager.GenerateChangeEmailTokenAsync(currentUser, request.changeEmailRequest.NewEmail);
        var tokenExpirationInMinutes = authSettings.Value.EmailTokenLifespanInMinutes;
        currentUser.EmailChangeTokenExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutes);
        currentUser.PendingNewEmail = request.changeEmailRequest.NewEmail;

        try
        {
            await emailService.SendEmailChangeTokenAsync(currentUser, request.changeEmailRequest.NewEmail,
                changeEmailToken);
            await userManager.UpdateAsync(currentUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,$"Error while changing email: {ex.Message}");
            throw new EmailException("Error while sending email change confirmation");
        }
    }
}