using Application.Exceptions;
using AutoMapper;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Register;

public class RegisterHandler(
    UserManager<User> userManager,
    IMapper mapper,
    IEmailService emailService,
    IOptions<AuthSettings> authSettings
    ) : IRequestHandler<RegisterCommand, int>
{
    public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<User>(request.registerDto);
        var result = await userManager.CreateAsync(user, request.registerDto.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new UserRegistrationException("Registration failed", errors);
        }

        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var tokenExpirationInMinutes = authSettings.Value.EmailTokenLifespanInMinutes;
        user.EmailConfirmationTokenExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutes);

        try
        {
            await emailService.SendConfirmationAsync(user, emailToken);
            await userManager.UpdateAsync(user);
        }
        catch (Exception)
        {
            await userManager.DeleteAsync(user);
            throw new EmailException("Error while sending confirmation email", []);
        }
        
        return user.Id;
    }
    
}