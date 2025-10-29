using MediatR;

namespace Application.Features.Users.RemoveProfilePicture;

public class RemoveProfilePictureCommand : IRequest
{
    public int UserId { get; set; }
}