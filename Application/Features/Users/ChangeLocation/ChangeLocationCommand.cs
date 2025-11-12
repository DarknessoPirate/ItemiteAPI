using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ChangeLocation;

public class ChangeLocationCommand : IRequest
{
    public int UserId { get; set; }
    public ChangeLocationRequest Dto { get; set; }
}