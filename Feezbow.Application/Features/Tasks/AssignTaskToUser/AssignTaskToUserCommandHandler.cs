using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.AssignTaskToUser;

public class AssignTaskToUserCommandHandler(
    IBoardTaskService boardTaskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AssignTaskToUserCommand, AssignTaskToUserCommandResponse>
{
    public async Task<AssignTaskToUserCommandResponse> Handle(AssignTaskToUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;

        // Get task before assignment to get old assigneeId
        var taskBefore = await boardTaskService.GetTaskByIdAsync(request.TaskId, userId, cancellationToken);
        var oldAssigneeId = taskBefore?.AssigneeId;

        await boardTaskService.AssignTaskToUserAsync(request.TaskId, request.newAssigneeId, userId, cancellationToken);
        
        await InvalidateCacheAsync(request.TaskId, oldAssigneeId, request.newAssigneeId, cancellationToken);
        
        return new AssignTaskToUserCommandResponse(Result<long>.Success(request.TaskId, "Task assigned to user"));
    }

    private async Task InvalidateCacheAsync(long taskId, long? oldAssigneeId, long newAssigneeId, CancellationToken ct)
    {
        var task = await boardTaskService.GetTaskByIdAsync(taskId, currentUserService.UserId!.Value, ct);
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.Task(taskId), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
            
            if (oldAssigneeId.HasValue)
            {
                await cacheService.RemoveAsync(CacheKeys.UserTasks(oldAssigneeId.Value), ct);
            }
            
            await cacheService.RemoveAsync(CacheKeys.UserTasks(newAssigneeId), ct);
        }
    }
}