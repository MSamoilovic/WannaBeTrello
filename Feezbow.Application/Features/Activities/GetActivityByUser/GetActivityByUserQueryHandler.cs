using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Activities.GetActivityByUser;

public class GetActivityByUserQueryHandler(
    IActivityLogService activityLogService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<GetActivityByUserQuery, IReadOnlyList<GetActivityByUserQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByUserQueryResponse>> Handle(GetActivityByUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = $"activity:user:{request.UserId}";
        
        var res = await cacheService.GetOrSetAsync(
            cacheKey,
            () => activityLogService.GetActivitiesForUserAsync(request.UserId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );

        return [.. (res ?? []).Select(act => GetActivityByUserQueryResponse.FromEntity(act))];
    }
}

