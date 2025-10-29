using Domain.Auth;
using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Auth.Login;

public class LoginCommand : IRequest<UserBasicResponse>
{
    public LoginRequest loginDto;
    public string IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
}