using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Activities.GetActivityByTask;

public class GetActivityByTaskQueryHandler(
    IActivityLogService activityLogService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<GetActivityByTaskQuery, IReadOnlyList<GetActivityByTaskQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByTaskQueryResponse>> Handle(GetActivityByTaskQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = $"activity:task:{request.TaskId}";
        
        var res = await cacheService.GetOrSetAsync(
            cacheKey,
            () => activityLogService.GetActivitiesForTaskAsync(request.TaskId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );

        return [.. (res ?? []).Select(act => GetActivityByTaskQueryResponse.FromEntity(act))];
    }
}

