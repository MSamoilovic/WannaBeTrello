using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.ReactivateUser;

public class ReactivateUserCommandHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService) 
    : IRequestHandler<ReactivateUserCommand, ReactivateUserCommandResponse>
{
    public async Task<ReactivateUserCommandResponse> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        await userService.ReactivateUserAsync(request.UserId, userId, cancellationToken);

        await InvalidateCacheAsync(request.UserId, cancellationToken);

        return new ReactivateUserCommandResponse(Result<long>.Success(request.UserId, "User Reactivated Successfully"));
    }

    private async Task InvalidateCacheAsync(long userId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.UserProfile(userId), ct);
    }
}
