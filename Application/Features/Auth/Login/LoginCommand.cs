using Domain.Auth;
using MediatR;

namespace Application.Features.Auth.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public LoginRequest loginDto;
    public string IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
}