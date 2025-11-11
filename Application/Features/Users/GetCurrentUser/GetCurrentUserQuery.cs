using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserResponse>
{
    public int UserId { get; set; }
}