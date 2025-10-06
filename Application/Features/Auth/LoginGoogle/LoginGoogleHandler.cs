using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Application.Features.Auth.LoginGoogle;

public class LoginGoogleHandler(
    SignInManager<User> signInManager,
    LinkGenerator linkGenerator,
    IHttpContextAccessor contextAccessor
    ) : IRequestHandler<LoginGoogleCommand, AuthenticationProperties>
{
    public async Task<AuthenticationProperties> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
    {
        var generatedUri = linkGenerator.GetPathByName(contextAccessor.HttpContext!,  "LoginGoogleCallback");
        var fullUri = generatedUri + $"?returnUrl={request.ReturnUrl}";
        
        var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", fullUri);
        return await Task.FromResult(properties);
    }
}