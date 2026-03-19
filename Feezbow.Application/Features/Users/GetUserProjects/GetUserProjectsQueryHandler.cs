using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.GetUserProjects;

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

