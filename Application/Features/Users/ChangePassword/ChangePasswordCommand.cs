using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ChangePassword;

public class ChangePasswordCommand : IRequest
{
   public int UserId { get; set; }
   public ChangePasswordRequest dto { get; set; }
}