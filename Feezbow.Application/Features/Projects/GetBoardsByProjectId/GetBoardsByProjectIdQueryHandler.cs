using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Projects.GetBoardsByProjectId;

public class GetBoardsByProjectIdQueryHandler(
    IBoardService boardService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetBoardsByProjectIdQuery, List<GetBoardsByProjectIdQueryResponse>>
{
    public async Task<List<GetBoardsByProjectIdQueryResponse>> Handle(GetBoardsByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var cacheKey = CacheKeys.ProjectBoards(request.ProjectId);
        var userId = currentUserService.UserId!.Value;

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            () => boardService.GetBoardByProjectIdAsync(request.ProjectId, userId, cancellationToken),
            CacheExpiration.Medium,
            cancellationToken
        );

        return result?.Select(board => new GetBoardsByProjectIdQueryResponse(board.Id, board.Name)).ToList() ?? [];
    }
}