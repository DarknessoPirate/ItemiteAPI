using System.Reflection;
using Domain.DTOs.Email;
using Domain.Entities;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailService(
    IFluentEmail fluentEmail,
    IConfiguration configuration
    ) : IEmailService
{
    public async Task SendConfirmationAsync(User user, string emailToken)
    {
        var authSettings = configuration.GetSection("AuthSettings");
        var queryParam = new Dictionary<string, string>
        {
            { "token", emailToken },
            { "email", user.Email! }
        };

        var emailConfirmationUri = authSettings.GetValue<string>("EmailVerificationUri") 
                                   ?? "https://localhost:4200/confirm-email";
        
        var confirmationLink = QueryHelpers.AddQueryString(emailConfirmationUri, queryParam!);
        
        var template = "Helpers/EmailTemplates/EmailConfirmation.cshtml";
        
        var sendResponse = await fluentEmail
            .To(user.Email!)
            .Subject("Itemite email confirmation")
            .UsingTemplateFromFile(template, new EmailConfirmationModel
            {
                UserName = user.UserName!,
                ConfirmationLink = confirmationLink
            })
            .SendAsync();

        if (!sendResponse.Successful)
        {
            throw new EmailException("Error while sending confirmation email", sendResponse.ErrorMessages.ToList());
        }
    }
}