using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Users.ChangeBackgroundPicture;

public class ChangeBackgroundPictureCommand : IRequest<string>
{
    public int UserId { get; set; }
    public FileWrapper File { get; set; }
}