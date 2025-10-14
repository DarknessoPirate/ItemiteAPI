using System.Net;
using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.ConfirmEmailChange;

public class ConfirmEmailChangeHandler(
    UserManager<User> userManager,
    ILogger<ConfirmEmailChangeHandler> logger
) : IRequestHandler<ConfirmEmailChangeCommand, string>
{
    public async Task<string> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
    {
        // get user with id
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        // check if token is for this user (confirm old email matches)
        if (user.Email != command.request.CurrentEmail)
            throw new UnauthorizedException("Email change token does not match the current user");

        // check if user has pending email change
        if (string.IsNullOrEmpty(user.PendingNewEmail))
            throw new BadRequestException("You do not have a pending email change request");

        // check if token expired
        if (user.EmailChangeTokenExpirationDate < DateTime.UtcNow)
            throw new BadRequestException("Email change token has expired");

        // check if email was taken in the time it took to activate the token
        var existingUser = await userManager.FindByEmailAsync(user.PendingNewEmail);
        if (existingUser != null)
            throw new BadRequestException("Email is already taken");

        var decodedToken = WebUtility.UrlDecode(command.request.Token);
        
        var result = await userManager.ChangeEmailAsync(user, user.PendingNewEmail, command.request.Token);

        if (!result.Succeeded)
            throw new BadRequestException("Invalid email change token");

        var newEmail = user.PendingNewEmail;
        user.PendingNewEmail = null;
        user.EmailChangeTokenExpirationDate = null;
        await userManager.UpdateAsync(user);
        
        
        return newEmail;
    }
}