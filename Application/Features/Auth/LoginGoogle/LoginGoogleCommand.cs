using MediatR;
using Microsoft.AspNetCore.Authentication;

namespace Application.Features.Auth.LoginGoogle;

public class LoginGoogleCommand : IRequest<AuthenticationProperties>
{
    public string ReturnUrl { get; set; }
}