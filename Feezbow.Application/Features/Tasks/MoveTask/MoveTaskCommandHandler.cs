using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Tasks.MoveTask;

public class MoveTaskCommandHandler(
    IBoardTaskService taskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<MoveTaskCommand, MoveTaskCommandResponse>
{
    public async Task<MoveTaskCommandResponse> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        await taskService.MoveTaskAsync(
            request.TaskId,
            request.NewColumnId,
            currentUserService.UserId.Value,
            cancellationToken
        );

        await InvalidateCacheAsync(request.TaskId, cancellationToken);

        return new MoveTaskCommandResponse(Result<long>.Success(request.TaskId, "Task moved successfully")); 
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        var task = await taskService.GetTaskByIdAsync(taskId, currentUserService.UserId!.Value, ct);
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.Task(taskId), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardColumns(boardId), ct);
        }
    }
}