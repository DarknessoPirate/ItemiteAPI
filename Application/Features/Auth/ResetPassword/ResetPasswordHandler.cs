using System.Net;
using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.ResetPassword;

public class ResetPasswordHandler(
        UserManager<User> userManager
    ) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.resetPasswordRequest.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var decodedToken = WebUtility.UrlDecode(request.resetPasswordRequest.Token);
        var result = await userManager.ResetPasswordAsync(user, decodedToken, request.resetPasswordRequest.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException("Password reset failed", result.Errors.Select(e => e.Description).ToList());
        }
    }
}