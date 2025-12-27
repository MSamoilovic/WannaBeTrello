using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.ReorderColumn;

public class ReorderColumnCommandHandler(
    IBoardService boardService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<ReorderColumnCommand, ReorderColumnCommandResponse>
{
    public async Task<ReorderColumnCommandResponse> Handle(ReorderColumnCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException();
        }

        await boardService.ReorderColumnsAsync(
            request.BoardId,
            request.ColumnOrders,
            currentUserService.UserId.Value,
            cancellationToken);

        await InvalidateCacheAsync(request.BoardId, cancellationToken);

        var result = Domain.Entities.Common.Result<long>.Success(request.BoardId, "Columns reordered successfully");
        return new ReorderColumnCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long boardId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.BoardColumns(boardId), ct);
    }
}