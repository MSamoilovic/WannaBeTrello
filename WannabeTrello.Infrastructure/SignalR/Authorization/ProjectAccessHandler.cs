using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.SignalR.Authorization;

/// <summary>
/// Handles <see cref="ProjectAccessRequirement"/> by checking project membership via the repository.
/// Results are cached in memory for 5 minutes to reduce database pressure on repeated hub calls.
/// </summary>
public sealed class ProjectAccessHandler(IServiceScopeFactory scopeFactory, IMemoryCache cache)
    : AuthorizationHandler<ProjectAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectAccessRequirement requirement)
    {
        if (!long.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return;
        }

        var cacheKey = $"ProjectAccess:{requirement.ProjectId}:{userId}";

        if (!cache.TryGetValue(cacheKey, out bool hasAccess))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var projectRepo = scope.ServiceProvider.GetRequiredService<IProjectRepository>();

            hasAccess = await projectRepo.IsProjectMemberAsync(requirement.ProjectId, userId);

            cache.Set(cacheKey, hasAccess, TimeSpan.FromMinutes(5));
        }

        if (hasAccess)
            context.Succeed(requirement);
    }
}
