using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ConfirmEmailChange;

public class ConfirmEmailChangeCommand : IRequest<string>
{
    public int UserId { get; set; }
    public Domain.DTOs.User.ChangeEmailConfirmRequest request { get; set; }
}