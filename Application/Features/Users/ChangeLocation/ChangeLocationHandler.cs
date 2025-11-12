using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.ChangeLocation;

public class ChangeLocationHandler(
    UserManager<User> userManager
    ) : IRequestHandler<ChangeLocationCommand>
{
    public async Task Handle(ChangeLocationCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException($"User with given Id: {request.UserId} not found");
        }
        
        user.Location = request.Dto.Location;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException($"Failed to change the location: {string.Join(",", errors)}");
        }
    }
}