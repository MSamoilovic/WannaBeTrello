using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Activities.GetActivityByBoard;

public class GetActivityByBoardQueryHandler(
    IActivityLogService activityLogService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetActivityByBoardQuery, IReadOnlyList<GetActivityByBoardQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByBoardQueryResponse>> Handle(GetActivityByBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var cacheKey = CacheKeys.BoardActivity(request.BoardId);
        
        var activities = await cacheService.GetOrSetAsync(
            cacheKey,
            () => activityLogService.GetActivitiesForBoardAsync(request.BoardId, cancellationToken),
            CacheExpiration.Short,  
            cancellationToken
        );

        return GetActivityByBoardQueryResponse.FromEntity(activities ?? []);
    }
}