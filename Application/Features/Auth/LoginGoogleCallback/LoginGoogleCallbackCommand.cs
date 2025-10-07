using System.Security.Claims;
using Domain.Auth;
using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Auth.LoginGoogleCallback;

public class LoginGoogleCallbackCommand : IRequest
{
    public string ReturnUrl { get; set; }
    public ClaimsPrincipal? ClaimsPrincipal { get; set; }
    public string IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
}