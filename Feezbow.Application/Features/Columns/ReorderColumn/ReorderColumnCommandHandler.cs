using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Columns.ReorderColumn;

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