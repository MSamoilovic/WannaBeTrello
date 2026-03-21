using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Entities;
using Feezbow.Infrastructure.Services;

namespace Feezbow.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{
    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token rejected: token not found or expired");
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        user.EnsureActive();

        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, jwtTokenService.GetRefreshTokenExpiry());
        await userManager.UpdateAsync(user);

        var accessToken = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        logger.LogDebug("Refresh token rotated for user {UserId}", user.Id);

        return new RefreshTokenCommandResponse(accessToken, newRefreshToken, user.RefreshTokenExpiresAt!.Value);
    }
}
