using Domain.DTOs.Auth;
using MediatR;

namespace Application.Features.Auth.ResetPassword;

public class ResetPasswordCommand : IRequest
{
    public ResetPasswordRequest resetPasswordRequest { get; set; }
}