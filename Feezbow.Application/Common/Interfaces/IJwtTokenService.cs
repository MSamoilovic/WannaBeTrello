using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}