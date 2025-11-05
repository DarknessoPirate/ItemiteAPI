using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth.LogoutFromAllDevices;

public class LogoutFromAllDevicesHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IHttpContextAccessor contextAccessor,
    ITokenService tokenService
    ) : IRequestHandler<LogoutFromAllDevicesCommand, string>
{
    public async Task<string> Handle(LogoutFromAllDevicesCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = contextAccessor.HttpContext.Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
            throw new BadRequestException("Refresh token not found in cookies");

        var tokenFromDb = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (tokenFromDb == null)
        {
            throw new NotFoundException("Refresh token not found in database");
        }
        
        await tokenService.RevokeAllUserTokensAsync(tokenFromDb.UserId, request.IpAddress);
        tokenService.RemoveTokensFromCookies(contextAccessor.HttpContext);
        
        return "Successfully logged out from all devices";
    }
}