using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserProjects;

public class GetUserProjectsQueryHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetUserProjectsQuery, GetUserProjectsQueryResponse>
{
    public async Task<GetUserProjectsQueryResponse> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.UserProjects(request.UserId);

        var projects = await cacheService.GetOrSetAsync(
            cacheKey,
            () => userService.GetUserProjectsAsync(request.UserId, cancellationToken),
            CacheExpiration.Medium,
            cancellationToken
        );

        return GetUserProjectsQueryResponse.FromEntities(projects ?? []);
    }
}

