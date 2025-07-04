using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}