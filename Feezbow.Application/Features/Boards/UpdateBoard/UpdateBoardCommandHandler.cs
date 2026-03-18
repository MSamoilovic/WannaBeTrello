using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandHandler(IBoardService boardService, ICurrentUserService currentUserService, ICacheService cacheService)
    : IRequestHandler<UpdateBoardCommand, UpdateBoardCommandResponse>
{
    public async Task<UpdateBoardCommandResponse> Handle(UpdateBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var updatedBoard = await boardService.UpdateBoardAsync(
            request.Id, 
            request?.Name, 
            request?.Description,
            currentUserService.UserId.Value
            , cancellationToken);
        
        await InvalidateBoardCacheAsync(request!.Id, updatedBoard.ProjectId, cancellationToken);
        
        return UpdateBoardCommandResponse.FromEntity(updatedBoard);
    }
    
    private async Task InvalidateBoardCacheAsync(
        long boardId, 
        long projectId, 
        CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Board(boardId), ct);
        await cacheService.RemoveAsync(CacheKeys.ProjectBoards(projectId), ct);
    }
}