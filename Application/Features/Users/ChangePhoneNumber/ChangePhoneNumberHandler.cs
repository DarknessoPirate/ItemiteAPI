using Domain.Entities;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.ChangePhoneNumber;

public class ChangePhoneNumberHandler(
    UserManager<User> userManager
    ) : IRequestHandler<ChangePhoneNumberCommand>
{
    public async Task Handle(ChangePhoneNumberCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException($"User with given Id: {request.UserId} not found");
        }
        
        user.PhoneNumber = request.Dto.PhoneNumber;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException($"Failed to change the phone number: {string.Join(",", errors)}");
        }
    }
}