using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.ResetPassword;

public class ForgotPasswordHandler(
    UserManager<User> userManager,
    IEmailService emailService
    ) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.forgotPasswordDto.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.AuthProvider == AuthProvider.Google)
        {
            throw new BadRequestException("This account uses google login", []);
        }
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailService.SendPasswordResetTokenAsync(user, token);
    }
}