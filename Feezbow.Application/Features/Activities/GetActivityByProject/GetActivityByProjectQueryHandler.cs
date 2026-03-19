using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Activities.GetActivityByProject;

public class GetActivityByProjectQueryHandler(
    IActivityLogService activityLogService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<GetActivityByProjectQuery, IReadOnlyList<GetActivityByProjectQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByProjectQueryResponse>> Handle(GetActivityByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = $"activity:project:{request.ProjectId}";
        
        var res = await cacheService.GetOrSetAsync(
            cacheKey,
            () => activityLogService.GetActivitiesForProjectAsync(request.ProjectId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );

        return [.. (res ?? []).Select(act => GetActivityByProjectQueryResponse.FromEntity(act))];
    }
}
