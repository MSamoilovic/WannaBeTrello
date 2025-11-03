using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

public class MoveTaskCommandHandler(IBoardTaskService taskService, ICurrentUserService currentUserService)
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

        return new MoveTaskCommandResponse(Result<long>.Success(request.TaskId, "Task moved successfully")); 
    }
}