using System.Security.Claims;
using Domain.DTOs.User;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor contextAccessor) : ICurrentUserService
{
    public int GetId()
    {
        var userId =  contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedException("User id not found");
        return int.Parse(userId);
    }

    public string GetUserName()
    {
        return contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name) ?? throw new UnauthorizedException("User username not found");
    }

    public string GetEmail()
    {
        return contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email) ?? throw new UnauthorizedException("User email not found");
    }
}