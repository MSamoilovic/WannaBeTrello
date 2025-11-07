using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.AssignTaskToUser;

public class AssignTaskToUserCommandHandler(IBoardTaskService boardTaskService, ICurrentUserService currentUserService)
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

        await boardTaskService.AssignTaskToUserAsync(request.TaskId, request.newAssigneeId, userId, cancellationToken);
        return new AssignTaskToUserCommandResponse(Result<long>.Success(request.TaskId, "Task assigned to user"));
    }
}