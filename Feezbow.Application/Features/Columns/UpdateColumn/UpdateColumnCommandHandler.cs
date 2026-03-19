using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Columns.UpdateColumn;

public class UpdateColumnCommandHandler(
    IColumnService columnService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateColumnCommand, UpdateColumnCommandResponse>
{
    public async Task<UpdateColumnCommandResponse> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            //return Result<UpdateColumnCommandResponse>.Failure("User is not authenticated.");
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var updatedColumn = await columnService.UpdateColumnAsync(
            request.ColumnId,
            request.NewName,
            request.WipLimit,
            currentUserService.UserId.Value,
            cancellationToken
        );

        await InvalidateCacheAsync(updatedColumn.BoardId, request.ColumnId, cancellationToken);

       return UpdateColumnCommandResponse.FromEntity(updatedColumn);
    }

    private async Task InvalidateCacheAsync(long boardId, long columnId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Column(columnId), ct);
        await cacheService.RemoveAsync(CacheKeys.BoardColumns(boardId), ct);
    }
}