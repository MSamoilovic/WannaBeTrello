using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.RestoreBoard;

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
        
        var board = await boardService.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        var boardId = await boardService.RestoreBoardAsync(request.BoardId, currentUserService.UserId.Value, cancellationToken);
        
        if (board is not null)
        {
            await InvalidateCacheAsync(request.BoardId, board.ProjectId, cancellationToken);
        }
        
        var result = Result<long>.Success(boardId, $"Board {request.BoardId} is now restored.");
        
        return new RestoreBoardCommandResponse(result);
    }
    
    private async Task InvalidateCacheAsync(long boardId, long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Board(boardId), ct);
        await cacheService.RemoveAsync(CacheKeys.ProjectBoards(projectId), ct);
    }
}