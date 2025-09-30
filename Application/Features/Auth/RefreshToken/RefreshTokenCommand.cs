using Domain.Auth;
using Domain.DTOs;
using Domain.DTOs.Auth;
using MediatR;

namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public TokenPairRequest TokenPair { get; set; }
}