using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.DeactivateUser;

public class DeactivateUserCommandHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<DeactivateUserCommand, DeactivateUserCommandResponse>
{
    public async Task<DeactivateUserCommandResponse> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue) 
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        await userService.DeactivateUserAsync(request.UserId, userId, cancellationToken);

        await InvalidateCacheAsync(request.UserId, cancellationToken);

        return new DeactivateUserCommandResponse(Result<long>.Success(request.UserId, "User Deactivated Successfully"));
    }

    private async Task InvalidateCacheAsync(long userId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.UserProfile(userId), ct);
    }
}
