using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.LockUser;

public class LockUserCommand : IRequest
{
    public LockUserRequest LockUserDto { get; set; }
}