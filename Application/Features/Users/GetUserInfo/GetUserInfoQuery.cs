using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.GetUserInfo;

public class GetUserInfoQuery : IRequest<UserResponse>
{
    public int UserId { get; set; }
}