using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.ChangePassword;

public class ChangePasswordHandler(
    UserManager<User> userManager,
    ILogger<ChangePasswordHandler> logger
    ) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Incorrect user ID");
        
        if (request.dto.OldPassword == request.dto.NewPassword)
            throw new BadRequestException("Passwords must be different");

        var result = await userManager.ChangePasswordAsync(user, request.dto.OldPassword, request.dto.NewPassword);

        if (!result.Succeeded)
        {
            var errorMessages = result.Errors.Select(e => e.Description).ToList();
            
            logger.LogError(
                "Failed to change password for user {UserId}. Errors: {Errors}", 
                request.UserId, 
                string.Join("; ", errorMessages));
            
            // Check if it's authentication issue or validation issue
            var isAuthError = result.Errors.Any(e => 
                e.Code == "PasswordMismatch" || 
                e.Code == "InvalidPassword");
            
            if (isAuthError)
            {
                throw new UnauthorizedException("Incorrect password");
            }
            else
            {
                // Password policy violations
                throw new BadRequestException(
                    "Failed to change password", 
                    errorMessages);
            }
        }
            
    }
}