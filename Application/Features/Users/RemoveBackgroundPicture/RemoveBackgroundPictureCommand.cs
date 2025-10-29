using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Users.RemoveBackgroundPicture;

public class RemoveBackgroundPictureCommand : IRequest
{
    public int UserId { get; set; }
}