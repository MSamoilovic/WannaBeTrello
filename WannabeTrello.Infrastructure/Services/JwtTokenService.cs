using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Infrastructure.Options;

namespace WannabeTrello.Infrastructure.Services;

public class JwtTokenService(IOptions<JwtOptions> _options, UserManager<User> userManager)
    : IJwtTokenService
{

    private  JwtOptions Options => _options.Value;
    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes( Options.Key ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(Options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            Options.Issuer,
            Options.Audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}