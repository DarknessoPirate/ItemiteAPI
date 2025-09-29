using System.Reflection;
using Domain.DTOs.Email;
using Domain.Entities;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;

namespace Infrastructure.Services;

public class EmailService(IFluentEmail fluentEmail) : IEmailService
{
    public async Task SendConfirmationAsync(EmailRequest emailRequest, string username, string confirmationLink)
    {
        var template = "Helpers/EmailTemplates/EmailConfirmation.cshtml";
        
        var sendResponse = await fluentEmail
            .To(emailRequest.ToAddress)
            .Subject(emailRequest.Subject)
            .UsingTemplateFromFile(template, new EmailConfirmationModel
            {
                UserName = username,
                ConfirmationLink = confirmationLink
            })
            .SendAsync();

        if (!sendResponse.Successful)
        {
            throw new EmailException("Error while sending confirmation email", sendResponse.ErrorMessages.ToList());
        }
    }
}