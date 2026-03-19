using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.GetUserAssignedTasks;

public class GetUserAssignedTasksQueryHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetUserAssignedTasksQuery, GetUserAssignedTasksQueryResponse>
{
    public async Task<GetUserAssignedTasksQueryResponse> Handle(GetUserAssignedTasksQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.UserTasks(request.UserId);

        var tasks = await cacheService.GetOrSetAsync(
            cacheKey,
            () => userService.GetUserAssignedTasksAsync(request.UserId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );

        return GetUserAssignedTasksQueryResponse.FromEntities(tasks ?? []);
    }
}

