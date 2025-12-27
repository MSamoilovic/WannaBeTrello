using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.ArchiveTask;

public class ArchiveTaskCommandHandler(
    IBoardTaskService boardTaskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<ArchiveTaskCommand, ArchiveTaskCommandResponse>
{
    public async Task<ArchiveTaskCommandResponse> Handle(ArchiveTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        
        // Get task before archiving to get boardId and assigneeId
        var task = await boardTaskService.GetTaskByIdAsync(request.TaskId, userId, cancellationToken);
        
        await boardTaskService.ArchiveTaskAsync(request.TaskId, userId, cancellationToken);
        
        await InvalidateCacheAsync(task, cancellationToken);
        
        return new ArchiveTaskCommandResponse(Result<long>.Success(request.TaskId, "Archived task"));
    }

    private async Task InvalidateCacheAsync(Domain.Entities.BoardTask? task, CancellationToken ct)
    {
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.Task(task.Id), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
            
            if (task.AssigneeId.HasValue)
            {
                await cacheService.RemoveAsync(CacheKeys.UserTasks(task.AssigneeId.Value), ct);
            }
        }
    }
}