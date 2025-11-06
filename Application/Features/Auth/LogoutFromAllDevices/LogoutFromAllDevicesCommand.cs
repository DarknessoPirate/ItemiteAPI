using MediatR;

namespace Application.Features.Auth.LogoutFromAllDevices;

public class LogoutFromAllDevicesCommand : IRequest<string>
{
    public string IpAddress { get; set; }
}