using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommandHandler(
    IBoardTaskService taskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateTaskCommand, UpdateTaskCommandResponse>
{
    public async Task<UpdateTaskCommandResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        await taskService.UpdateTaskDetailsAsync(
            request.TaskId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            currentUserService.UserId.Value,
            cancellationToken
        );

        await InvalidateCacheAsync(request.TaskId, cancellationToken);

        return new UpdateTaskCommandResponse(Result<long>.Success(request.TaskId, "Task updated successfully"));
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        var task = await taskService.GetTaskByIdAsync(taskId, currentUserService.UserId!.Value, ct);
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.Task(taskId), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
            
            if (task.AssigneeId.HasValue)
            {
                await cacheService.RemoveAsync(CacheKeys.UserTasks(task.AssigneeId.Value), ct);
            }
        }
    }
}