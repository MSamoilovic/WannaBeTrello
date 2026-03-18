using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandHandler(
    IBoardTaskService taskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateTaskCommand, CreateTaskCommandResponse>
{
    public async Task<CreateTaskCommandResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }

        var task = await taskService.CreateTaskAsync(
            request.ColumnId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Position,
            request.AssigneeId,
            currentUserService.UserId.Value,
            cancellationToken
        );

        await InvalidateCacheAsync(task.Id, request.ColumnId, request.AssigneeId, cancellationToken);

        return new CreateTaskCommandResponse(Result<long>.Success(task.Id, "Task created successfully"));
    }

    private async Task InvalidateCacheAsync(long taskId, long columnId, long? assigneeId, CancellationToken ct)
    {
        
        var task = await taskService.GetTaskByIdAsync(taskId, currentUserService.UserId!.Value, ct);
        if (task != null)
        {
            var boardId = task.Column.BoardId;
            
            await cacheService.RemoveAsync(CacheKeys.BoardTasks(boardId), ct);
            
            if (assigneeId.HasValue)
            {
                await cacheService.RemoveAsync(CacheKeys.UserTasks(assigneeId.Value), ct);
            }
        }
    }
}
