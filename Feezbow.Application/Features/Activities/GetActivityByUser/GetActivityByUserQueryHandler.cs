using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Activities.GetActivityByUser;

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

