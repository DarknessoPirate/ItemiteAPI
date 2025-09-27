using Domain.Auth;
using MediatR;

namespace Application.Features.Auth.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    private LoginRequest loginDto;
}