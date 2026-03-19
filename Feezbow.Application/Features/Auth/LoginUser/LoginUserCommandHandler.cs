using MediatR;
using Microsoft.AspNetCore.Identity;
using Feezbow.Domain.Entities;
using Feezbow.Infrastructure.Services;

namespace Feezbow.Application.Features.Auth.LoginUser;

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

        user.EnsureActive();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Neispravno korisničko ime ili lozinka.");
        }

        user.UpdateLastLogin();
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, jwtTokenService.GetRefreshTokenExpiry());
        await userManager.UpdateAsync(user);

        var token = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);
        return new LoginUserCommandResponse(token, user.Email, user.EmailConfirmed, refreshToken, user.RefreshTokenExpiresAt!.Value);
    }
}