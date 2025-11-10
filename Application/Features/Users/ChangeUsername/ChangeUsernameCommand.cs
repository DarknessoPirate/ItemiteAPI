using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ChangeUsername;

public class ChangeUsernameCommand : IRequest
{
    public int UserId { get; set; }
    public ChangeUsernameRequest Dto { get; set; }
}