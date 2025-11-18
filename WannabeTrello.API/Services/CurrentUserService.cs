using System.Security.Claims;
using WannabeTrello.Application.Common.Interfaces;

namespace WannabeTrello.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public long? UserId
    {
        get
        {
            var idClaim = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && long.TryParse(idClaim.Value, out long id))
            {
                return id;
            }
            
            return null;    
        }
    }

    public string? UserName { get; }

    public bool IsAuthenticated => httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string UserIPAddress => httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}