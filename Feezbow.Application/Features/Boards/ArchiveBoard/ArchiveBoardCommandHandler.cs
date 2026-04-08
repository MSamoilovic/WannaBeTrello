using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Boards.ArchiveBoard;

public class ArchiveBoardCommandHandler(
    IBoardService boardService, 
    ICurrentUserService currentUserService, 
    ICacheService cacheService)
    : IRequestHandler<ArchiveBoardCommand, ArchiveBoardCommandResponse>
{
    public async Task<ArchiveBoardCommandResponse> Handle(ArchiveBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        // Fetch board details BEFORE archiving so we have the projectId for cache invalidation.
        // After archiving, the global query filter (!b.IsArchived) would exclude the board.
        var board = await boardService.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);

        var boardId =
            await boardService.ArchiveBoardAsync(request.BoardId, currentUserService.UserId.Value, cancellationToken);

        await InvalidateCacheAsync(board.ProjectId, boardId, cancellationToken);

        var result = Result<long>.Success(boardId, $"Board {request.BoardId} is now archived.");

        return new ArchiveBoardCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, long boardId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Board(boardId), ct);
        await cacheService.RemoveAsync(CacheKeys.ProjectBoards(projectId), ct);
        await cacheService.RemoveAsync(CacheKeys.BoardColumns(boardId), ct);
        await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
    }
}