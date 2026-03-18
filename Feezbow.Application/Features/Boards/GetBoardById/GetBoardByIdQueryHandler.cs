using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

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