using Application.Exceptions;
using AutoMapper;
using Domain.DTOs.Email;
using Domain.Entities;
using FluentEmail.Core;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Features.Auth.Register;

public class RegisterHandler(
    UserManager<User> userManager,
    IMapper mapper,
    IEmailService emailService
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
        var queryParam = new Dictionary<string, string>
        {
            { "token", emailToken },
            { "email", user.Email! }
        };

        var confirmationLink = QueryHelpers.AddQueryString(request.registerDto.EmailVerificationUri, queryParam);
        var emailRequest = new EmailRequest()
        {
            ToAddress = user.Email!,
            Subject = "Itemite email confirmation",
        };
        
        await emailService.SendConfirmationAsync(emailRequest, user.UserName!, confirmationLink);
        
        return user.Id;
    }
    
}