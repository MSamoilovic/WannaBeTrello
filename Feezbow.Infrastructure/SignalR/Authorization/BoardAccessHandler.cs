using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.SignalR.Authorization;

/// <summary>
/// Handles <see cref="BoardAccessRequirement"/> by checking board membership via the repository.
/// Results are cached in memory for 5 minutes to reduce database pressure on repeated hub calls.
/// </summary>
public sealed class BoardAccessHandler(IServiceScopeFactory scopeFactory, IMemoryCache cache)
    : AuthorizationHandler<BoardAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BoardAccessRequirement requirement)
    {
        if (!long.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return;
        }

        var cacheKey = $"BoardAccess:{requirement.BoardId}:{userId}";

        if (!cache.TryGetValue(cacheKey, out bool hasAccess))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var boardRepo = scope.ServiceProvider.GetRequiredService<IBoardRepository>();

            var board = await boardRepo.GetBoardWithDetailsAsync(requirement.BoardId);
            hasAccess = board != null && board.BoardMembers.Any(bm => bm.UserId == userId);

            cache.Set(cacheKey, hasAccess, TimeSpan.FromMinutes(5));
        }

        if (hasAccess)
            context.Succeed(requirement);
    }
}
