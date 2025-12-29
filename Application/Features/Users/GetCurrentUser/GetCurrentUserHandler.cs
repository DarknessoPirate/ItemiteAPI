using AutoMapper;
using Domain.DTOs.User;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.GetCurrentUser;

public class GetCurrentUserHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithAllFieldsAsync(request.UserId);
        if (user == null)
        {
            throw new UnauthorizedException("User not found");
        }
        
        var userRoleDict = await userRepository.GetUserRolesAsync([user.Id]);
        var mappedUser = mapper.Map<UserResponse>(user);
        
        mappedUser.Roles = userRoleDict.TryGetValue(mappedUser.Id, out var roles) ?  roles : new List<string>();
        
        return mappedUser;
    }
}