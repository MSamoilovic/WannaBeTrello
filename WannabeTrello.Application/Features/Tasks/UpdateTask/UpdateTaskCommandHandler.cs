using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommandHandler(IBoardTaskService taskService, ICurrentUserService currentUserService)
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

        return new UpdateTaskCommandResponse(Result<long>.Success(request.TaskId, "Task updated successfully"));
    }
}