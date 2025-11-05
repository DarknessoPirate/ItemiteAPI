using MediatR;

namespace Application.Features.Auth.Logout;

public class LogoutCommand : IRequest<string>
{
    public string IpAddress { get; set; }
}