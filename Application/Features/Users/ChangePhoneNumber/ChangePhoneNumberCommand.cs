using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.ChangePhoneNumber;

public class ChangePhoneNumberCommand : IRequest
{
    public int UserId { get; set; }
    public ChangePhoneNumberRequest Dto { get; set; }
}