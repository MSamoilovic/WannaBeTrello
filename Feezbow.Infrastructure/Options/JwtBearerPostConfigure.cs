using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WannabeTrello.Infrastructure.Options;

public class JwtBearerPostConfigure : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtOptions> _jwtOptions;

    public JwtBearerPostConfigure(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var jwtOptions = _jwtOptions.Value;

        options.TokenValidationParameters.ValidIssuer = jwtOptions.Issuer;
        options.TokenValidationParameters.ValidAudience = jwtOptions.Audience;
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.Key));
    }
}
