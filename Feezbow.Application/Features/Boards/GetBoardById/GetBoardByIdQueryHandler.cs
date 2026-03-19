using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryHandler(IBoardService boardService, ICurrentUserService currentUserService, ICacheService cacheService)
    : IRequestHandler<GetBoardByIdQuery, GetBoardByIdQueryResponse>
{
    public async Task<GetBoardByIdQueryResponse> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("User is not authenticated");
        }

        var cacheKey = CacheKeys.Board(request.BoardId);

        var userId = currentUserService.UserId.Value;
        var board = await cacheService.GetOrSetAsync(
             cacheKey,
             () => boardService.GetBoardWithDetailsAsync(request.BoardId, cancellationToken),
             CacheExpiration.Medium,
             cancellationToken
         );

        if (board is null)
        {
            throw new NotFoundException(nameof(Board), request.BoardId);
        }

        return GetBoardByIdQueryResponse.FromEntity(board);
    }
}