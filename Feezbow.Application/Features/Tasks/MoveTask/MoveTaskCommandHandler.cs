using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

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