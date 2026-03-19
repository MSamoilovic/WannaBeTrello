using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.GetUserBoards;

public class GetUserBoardsQueryHandler(
    IUserService userService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetUserBoardsQuery, GetUserBoardsQueryResponse>
{
    public async Task<GetUserBoardsQueryResponse> Handle(GetUserBoardsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.UserBoards(request.UserId);

        var boards = await cacheService.GetOrSetAsync(
            cacheKey,
            () => userService.GetUserBoardMemberships(request.UserId, cancellationToken),
            CacheExpiration.Medium,
            cancellationToken
        );

        return GetUserBoardsQueryResponse.FromEntities(boards ?? []);
    }
}

