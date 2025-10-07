using Domain.Auth;
using Domain.DTOs;
using Domain.DTOs.Auth;
using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<UserBasicResponse>
{
    public string IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
}