using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.UnlockUser;

public class UnlockUserCommand : IRequest
{
    public UnlockUserRequest UnlockUserDto { get; set; }
}