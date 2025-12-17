using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public class GetColumnsByBoardIdQueryHandler(IBoardService boardService, ICurrentUserService currentUserService, ICacheService cacheService)
    : IRequestHandler<GetColumnsByBoardIdQuery, List<GetColumnsByBoardIdQueryResponse>>
{
    public async Task<List<GetColumnsByBoardIdQueryResponse>> Handle(GetColumnsByBoardIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.BoardColumns(request.BoardId);

        var userId = currentUserService.UserId!.Value;

        var columns = await cacheService.GetOrSetAsync(
             cacheKey,
             () => boardService.GetColumnsByBoardIdAsync(
                 request.BoardId,
                 userId,
                 cancellationToken
             ),
             CacheExpiration.Medium,
             cancellationToken
         );

        if (columns is null)
        {
            throw new NotFoundException("Board", request.BoardId);
        }

        return GetColumnsByBoardIdQueryResponse.FromEntity(columns);
    }
}