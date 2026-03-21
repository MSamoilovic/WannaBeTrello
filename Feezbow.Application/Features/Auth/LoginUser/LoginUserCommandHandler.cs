using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Entities;
using Feezbow.Infrastructure.Services;

namespace Feezbow.Application.Features.Auth.LoginUser;

public class LoginUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtTokenService jwtTokenService,
    ILogger<LoginUserCommandHandler> logger)
    : IRequestHandler<LoginUserCommand, LoginUserCommandResponse>
{
    public async Task<LoginUserCommandResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UsernameOrEmail) ?? await userManager.FindByEmailAsync(request.UsernameOrEmail);
        if (user == null)
        {
            logger.LogWarning("Login failed: user not found for {UsernameOrEmail}", request.UsernameOrEmail);
            throw new UnauthorizedAccessException("Neispravno korisničko ime ili lozinka.");
        }

        user.EnsureActive();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: invalid password for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Neispravno korisničko ime ili lozinka.");
        }

        user.UpdateLastLogin();
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, jwtTokenService.GetRefreshTokenExpiry());
        await userManager.UpdateAsync(user);

        var token = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new LoginUserCommandResponse(token, user.Email, user.EmailConfirmed, refreshToken, user.RefreshTokenExpiresAt!.Value);
    }
}