using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserAssignedTasks;

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

