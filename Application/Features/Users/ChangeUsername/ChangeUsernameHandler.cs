using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.ChangeUsername;

public class ChangeUsernameHandler(
    UserManager<User> userManager
    ) : IRequestHandler<ChangeUsernameCommand>
{
    public async Task Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException($"User with given Id: {request.UserId} not found");
        }
        
        user.UserName = request.Dto.NewUsername;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException($"Failed to change the username: {string.Join(",", errors)}");
        }
    }
}