using AutoMapper;
using Domain.DTOs.User;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Users.GetUserInfo;

public class GetUserInfoHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetUserInfoQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithAllFieldsAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        return mapper.Map<UserResponse>(user);
    }
}