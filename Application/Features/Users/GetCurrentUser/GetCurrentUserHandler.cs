using AutoMapper;
using Domain.DTOs.User;
using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.GetCurrentUser;

public class GetCurrentUserHandler(
    UserManager<User> userManager,
    IMapper mapper
    ) : IRequestHandler<GetCurrentUserQuery, UserBasicResponse>
{
    public async Task<UserBasicResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new UnauthorizedException("User not found");
        }
        return mapper.Map<UserBasicResponse>(user);
    }
}