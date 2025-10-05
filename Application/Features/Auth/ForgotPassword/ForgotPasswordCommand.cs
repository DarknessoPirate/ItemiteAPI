using MediatR;
using Microsoft.AspNetCore.Identity.Data;

namespace Application.Features.Auth.ResetPassword;

public class ForgotPasswordCommand : IRequest
{
    public ForgotPasswordRequest forgotPasswordDto;
}