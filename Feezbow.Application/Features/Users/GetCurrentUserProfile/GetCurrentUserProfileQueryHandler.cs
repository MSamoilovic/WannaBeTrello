using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<GetCurrentUserProfileQuery, GetCurrentUserProfileQueryResponse>
{
    public async Task<GetCurrentUserProfileQueryResponse> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        if(!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;
        var cacheKey = CacheKeys.UserProfile(userId);

        var user = await cacheService.GetOrSetAsync(
            cacheKey,
            () => userService.GetUserProfileAsync(userId, cancellationToken),
            CacheExpiration.Long,
            cancellationToken
        );

        return GetCurrentUserProfileQueryResponse.FromEntity(user!);
    }
}
