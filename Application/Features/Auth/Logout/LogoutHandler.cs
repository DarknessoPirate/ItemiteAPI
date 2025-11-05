using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth.Logout;

public class LogoutHandler(
    IHttpContextAccessor contextAccessor,
    ITokenService tokenService
    ) : IRequestHandler<LogoutCommand, string>
{
    public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = contextAccessor.HttpContext.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new BadRequestException("Refresh token not found in cookies");
        }
        
        await tokenService.RevokeTokenAsync(refreshToken, request.IpAddress);
        
        tokenService.RemoveTokensFromCookies(contextAccessor.HttpContext);
        
        return "Successfully logged out";
    }
}