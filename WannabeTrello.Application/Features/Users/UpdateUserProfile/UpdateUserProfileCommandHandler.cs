using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.UpdateUserProfile;

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
