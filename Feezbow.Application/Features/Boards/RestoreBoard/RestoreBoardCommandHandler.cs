using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Boards.RestoreBoard;

public class RestoreBoardCommandHandler(
    IBoardService boardService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RestoreBoardCommand, RestoreBoardCommandResponse>
{
    public async Task<RestoreBoardCommandResponse> Handle(RestoreBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var boardId = await boardService.RestoreBoardAsync(request.BoardId, currentUserService.UserId.Value, cancellationToken);

        // Fetch board AFTER restoring — global filter (!IsArchived) now allows the query.
        var board = await boardService.GetBoardWithDetailsAsync(boardId, cancellationToken);
        await InvalidateCacheAsync(boardId, board.ProjectId, cancellationToken);
        
        var result = Result<long>.Success(boardId, $"Board {request.BoardId} is now restored.");
        
        return new RestoreBoardCommandResponse(result);
    }
    
    private async Task InvalidateCacheAsync(long boardId, long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Board(boardId), ct);
        await cacheService.RemoveAsync(CacheKeys.ProjectBoards(projectId), ct);
    }
}