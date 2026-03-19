using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Boards.GetColumnsByBoardIId;

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