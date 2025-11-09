using System.Security.Claims;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.LoginGoogleCallback;

public class LoginGoogleCallbackHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IHttpContextAccessor contextAccessor
        ) : IRequestHandler<LoginGoogleCallbackCommand, int>
{
    public async Task<int> Handle(LoginGoogleCallbackCommand request, CancellationToken cancellationToken)
    {
        if (request.ClaimsPrincipal == null)
        {
            return (int) GoogleLoginResult.NoClaimsFailure;
        }
        var email = request.ClaimsPrincipal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
           return (int) GoogleLoginResult.NoEmailFailure;
        }
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var userName = $"{email.Split('@')[0].ToLower()}_{Guid.NewGuid().ToString()[..8]}";
            user = new User
            {
                Email = email,
                UserName = userName,
                EmailConfirmed = true,
                AuthProvider = AuthProvider.Google
            };
            
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return (int) GoogleLoginResult.UsernameUniqueFailure;
            }
        }
        else if (user.AuthProvider == AuthProvider.Email)
        {
            return (int) GoogleLoginResult.EmailUniqueFailure;
        }
        var tokens = await tokenService.GenerateTokenPairAsync(
            user,
            request.IpAddress,
            request.DeviceId,
            request.UserAgent
        );
        
        tokenService.SetTokensInsideCookie(tokens, contextAccessor.HttpContext!);
        return (int) GoogleLoginResult.Success;
    }
}