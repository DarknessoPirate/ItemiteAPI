using Domain.DTOs.File;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.ChangeProfilePicture;

public class ChangeProfilePictureCommand : IRequest<string>
{
    public int UserId { get; set; }
    public FileWrapper File { get; set; }
}