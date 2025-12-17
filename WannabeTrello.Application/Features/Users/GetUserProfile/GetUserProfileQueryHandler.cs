using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserProfile;

public class GetUserProfileQueryHandler(IUserService userService, ICurrentUserService currentUserService, ICacheService cacheService) : IRequestHandler<GetUserProfileQuery, GetUserProfileQueryResponse>
{
    public async Task<GetUserProfileQueryResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue) 
        { 
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.UserProfile(request.UserId);

        var user = await cacheService.GetOrSetAsync(
             cacheKey,
             () => userService.GetUserProfileAsync(request.UserId, cancellationToken),
             CacheExpiration.Long,
             cancellationToken
         );

        return GetUserProfileQueryResponse.FromEntity(user!);
    }
}
