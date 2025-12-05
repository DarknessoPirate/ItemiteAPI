using Application.Exceptions;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.LockUser;

public class LockUserHandler(
    UserManager<User> userManager,
    IEmailService emailService
    ) : IRequestHandler<LockUserCommand>
{
    public async Task Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var userToLockout = await userManager.FindByIdAsync(request.LockUserDto.UserToLockoutId.ToString());
        if (userToLockout == null)
        {
            throw new Exception("User not found");
        }

        if (!userToLockout.LockoutEnabled)
        {
            throw new ForbiddenException("User can't be locked");
        }
        
        var lockoutDate = request.LockUserDto.LockoutEnd ?? DateTime.UtcNow.AddDays(3);
        userToLockout.LockoutEnd = lockoutDate;
        
        var result = await userManager.UpdateAsync(userToLockout);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException("Failed to lock user", errors);
        }
        
        await emailService.SendNotificationAsync(userToLockout, "Account locked", $"Your account has been locked until {lockoutDate.ToString("g")}", request.LockUserDto.LockoutMessage ?? string.Empty );
    }
}