using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}