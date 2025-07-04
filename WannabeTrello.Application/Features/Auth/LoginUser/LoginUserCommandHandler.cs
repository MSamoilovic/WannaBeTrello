using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Infrastructure.Services;

namespace WannabeTrello.Application.Features.Auth.LoginUser;

public class LoginUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, LoginUserCommandResponse>
{

    public async Task<LoginUserCommandResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UsernameOrEmail) ?? await userManager.FindByEmailAsync(request.UsernameOrEmail);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Neispravno korisničko ime ili lozinka.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Neispravno korisničko ime ili lozinka.");
        }

        var token = jwtTokenService.GenerateToken(user);
        return new LoginUserCommandResponse(token);
    }
}