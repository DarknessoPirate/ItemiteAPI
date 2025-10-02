using Domain.Auth;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Login;

public class LoginHandler(
    IJwtService jwtService,
    UserManager<User> userManager,
    IConfiguration configuration,
    IEmailService emailService
    ) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.loginDto.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!await userManager.CheckPasswordAsync(user, request.loginDto.Password))
        {
            throw new UnauthorizedException("Invalid password");
        }

        var authSettings = configuration.GetSection("AuthSettings");
        
        // checks the appsettings.json if email confirmation is needed to log in
        var isEmailConfRequired = authSettings.GetValue<bool>("IsEmailConfirmationRequired");
        
        if (isEmailConfRequired && !await userManager.IsEmailConfirmedAsync(user))
        {
            if (user.EmailConfirmationTokenExpirationDate < DateTime.UtcNow)
            {
                var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var tokenExpirationInMinutes = authSettings.GetValue<int>("EmailTokenLifespanInMinutes");
                user.EmailConfirmationTokenExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutes);
                try
                {
                    await emailService.SendConfirmationAsync(user, emailToken);
                    await userManager.UpdateAsync(user);
                }
                catch (Exception)
                {
                    throw new EmailException("Error while sending confirmation email", []);
                }
                throw new UnauthorizedException("Email is not confirmed. New confirmation link has been sent.");
            }
            
            throw new UnauthorizedException("Email is not confirmed. Check your email for the confirmation link");
            
        }
        
        var tokenResponse = jwtService.GenerateToken(user);
        var refreshTokenResponse = jwtService.GenerateRefreshToken();
        
        user.RefreshToken = refreshTokenResponse.Token;
        user.RefreshTokenExpirationDate = refreshTokenResponse.TokenExpirationDate;

        await userManager.UpdateAsync(user);

        return new AuthResponse(user, tokenResponse, refreshTokenResponse);
    }
}