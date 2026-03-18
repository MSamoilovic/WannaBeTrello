using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.RestoreTask;

public class RestoreTaskCommandHandler(
    IBoardTaskService boardTaskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RestoreTaskCommand, RestoreTaskCommandResponse>
{
    public async Task<RestoreTaskCommandResponse> Handle(RestoreTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        
        await boardTaskService.RestoreTaskAsync(request.TaskId, userId, cancellationToken);
        
        await InvalidateCacheAsync(request.TaskId, cancellationToken);
        
        return new RestoreTaskCommandResponse(Result<long>.Success(request.TaskId, "Restored task"));
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        var task = await boardTaskService.GetTaskByIdAsync(taskId, currentUserService.UserId!.Value, ct);
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.Task(taskId), ct);
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
        }
    }
}