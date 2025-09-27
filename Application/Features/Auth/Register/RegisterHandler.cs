using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Register;

public class RegisterHandler(
    UserManager<User> userManager,
    IMapper mapper
    ) : IRequestHandler<RegisterCommand, int>
{
    public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<User>(request.registerDto);
        var result = await userManager.CreateAsync(user, request.registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new UserRegistrationException("Registration failed", errors);
        }
        
        return user.Id;
    }
}