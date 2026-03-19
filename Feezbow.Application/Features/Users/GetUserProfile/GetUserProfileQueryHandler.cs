using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.GetUserProfile;

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

        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        return GetUserProfileQueryResponse.FromEntity(user!);
    }
}
