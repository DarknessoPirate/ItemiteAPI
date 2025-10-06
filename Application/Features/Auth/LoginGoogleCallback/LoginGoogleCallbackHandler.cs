using System.Security.Claims;
using Domain.Auth;
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
        ) : IRequestHandler<LoginGoogleCallbackCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginGoogleCallbackCommand request, CancellationToken cancellationToken)
    {
        if (request.ClaimsPrincipal == null)
        {
            throw new UnauthorizedException("No claims present");
        }
        var email = request.ClaimsPrincipal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            throw new UnauthorizedException("No email address present");
        }
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var userName = email.Split('@')[0].ToLower();
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
                throw new UnauthorizedException("Error creating user");
            }
        }
        else if (user.AuthProvider == AuthProvider.Email)
        {
            throw new UnauthorizedException("This email is already registered. Sign in using login form.");
        }
        var tokens = await tokenService.GenerateTokenPairAsync(
            user,
            request.IpAddress,
            request.DeviceId,
            request.UserAgent
        );
        
        var authResponse = new AuthResponse(user, tokens);
        
        tokenService.SetTokensInsideCookie(authResponse, contextAccessor.HttpContext!);

        return authResponse;
    }
}