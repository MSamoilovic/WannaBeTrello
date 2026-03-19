using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Tasks.UpdateTask;

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