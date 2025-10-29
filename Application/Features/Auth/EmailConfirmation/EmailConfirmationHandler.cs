using System.Net;
using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.EmailConfirmation;

public class EmailConfirmationHandler(
        UserManager<User> userManager
    ) : IRequestHandler<EmailConfirmationCommand>
{
    public async Task Handle(EmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.EmailConfirmationRequest.Email);
        if (user == null)
        {
            throw new BadRequestException("User not found");
        }
        
        if (user.EmailConfirmed)
        {
            throw new BadRequestException("Email already confirmed", []);
        }
        
        var decodedToken = WebUtility.UrlDecode(request.EmailConfirmationRequest.Token);
        var confirmation = await userManager.ConfirmEmailAsync(user, decodedToken);
        
        if (!confirmation.Succeeded)
        {
            throw new UnauthorizedException("Invalid email confirmation token");
        }
    }
}