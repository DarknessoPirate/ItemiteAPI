using Domain.DTOs.Auth;
using MediatR;

namespace Application.Features.Auth.EmailConfirmation;

public class EmailConfirmationCommand : IRequest
{
    public EmailConfirmationRequest EmailConfirmationRequest { get; set; }
}