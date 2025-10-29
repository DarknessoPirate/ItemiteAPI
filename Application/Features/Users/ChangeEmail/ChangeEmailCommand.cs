using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ChangeEmail;

public class ChangeEmailCommand : IRequest
{
    public int UserId { get; set; }
    public ChangeEmailRequest changeEmailRequest { get; set; }
}