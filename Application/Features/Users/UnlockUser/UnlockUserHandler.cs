using Application.Exceptions;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.UnlockUser;

public class UnlockUserHandler(
    UserManager<User> userManager,
    IEmailService emailService
    ) : IRequestHandler<UnlockUserCommand>
{
    public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var userToUnlock = await userManager.FindByIdAsync(request.UnlockUserDto.UserId.ToString());
        if (userToUnlock == null)
        {
            throw new Exception("User not found");
        }

        if (userToUnlock.LockoutEnd == null)
        {
            throw new ForbiddenException("User is not locked");
        }

        userToUnlock.LockoutEnd = null;
        var result = await userManager.UpdateAsync(userToUnlock);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException("Failed to unlock user", errors);
        }
        
        await emailService.SendNotificationAsync(userToUnlock, "Account unlocked", $"Your account has been unlocked!", request.UnlockUserDto.UnlockMessage ?? string.Empty);
    }
}