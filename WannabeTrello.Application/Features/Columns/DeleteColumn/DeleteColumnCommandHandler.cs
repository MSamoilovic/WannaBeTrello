using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public class DeleteColumnCommandHandler(
    IColumnService columnService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteColumnCommand, DeleteColumnCommandResponse>
{
    public async Task<DeleteColumnCommandResponse> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var userId = currentUserService.UserId.Value;
        
        // Get column before deletion to get BoardId
        var column = await columnService.GetColumnByIdAsync(request.ColumnId, userId, cancellationToken);
        
        var columnId = await columnService.DeleteColumnAsync(request.ColumnId, userId, cancellationToken);
        
        await InvalidateCacheAsync(column.BoardId, request.ColumnId, cancellationToken);
        
        var result = Result<long>.Success(columnId, "Column deleted successfully");
        return new DeleteColumnCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long boardId, long columnId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Column(columnId), ct);
        await cacheService.RemoveAsync(CacheKeys.BoardColumns(boardId), ct);
    }
}