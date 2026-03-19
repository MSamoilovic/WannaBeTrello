using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileCommandResponse>
{
    public async Task<UpdateUserProfileCommandResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
       if(!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

       var userId = currentUserService.UserId.Value;

       await userService.UpdateUserProfileAsync(
           request.UserId, 
           request.FirstName, 
           request.LastName, 
           request.Bio, request.ProfilePictureUrl!,
           userId, 
           cancellationToken
       );

        await InvalidateCacheAsync(request.UserId, cancellationToken);

        return new UpdateUserProfileCommandResponse(Result<long>.Success(request.UserId, "User Profile updated successfully"));
    }

    private async Task InvalidateCacheAsync(long userId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.UserProfile(userId), ct);
    }
}
