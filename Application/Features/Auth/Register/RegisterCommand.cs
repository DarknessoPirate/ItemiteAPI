using Domain.Auth;
using MediatR;

namespace Application.Features.Auth.Register;

public class RegisterCommand : IRequest<int>
{
    public RegisterRequest registerDto;
}